﻿using NLog;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Zip;
using UniJSON;


namespace GltfScene
{
    public struct Source
    {
        public string Path;
        public IStorage IO;
        public glTF GLTF;
        public ListTreeNode<JsonValue> JSON;

        public ArraySegment<byte> GetImageBytes(glTFImage image)
        {
            if (string.IsNullOrEmpty(image.uri))
            {
                return GLTF.GetViewBytes(IO, image.bufferView);
            }
            else
            {
                return IO.Get(image.uri);
            }
        }

        bool HasSameBuffer(glTFPrimitives lhs, glTFPrimitives rhs)
        {
            {
                var l = GLTF.accessors[lhs.indices];
                var r = GLTF.accessors[rhs.indices];
                if (l.componentType != r.componentType)
                {
                    return false;
                }
                if (l.type != r.type)
                {
                    return false;
                }
                if (l.bufferView != r.bufferView)
                {
                    return false;
                }
            }

            if (lhs.attributes.POSITION != rhs.attributes.POSITION) return false;
            if (lhs.attributes.NORMAL != rhs.attributes.NORMAL) return false;
            if (lhs.attributes.TEXCOORD_0 != rhs.attributes.TEXCOORD_0) return false;
            if (lhs.attributes.TANGENT != rhs.attributes.TANGENT) return false;
            if (lhs.attributes.COLOR_0 != rhs.attributes.COLOR_0) return false;
            if (lhs.attributes.JOINTS_0 != rhs.attributes.JOINTS_0) return false;
            if (lhs.attributes.WEIGHTS_0 != rhs.attributes.WEIGHTS_0) return false;

            return true;
        }

        public bool HasSameBuffer(IEnumerable<glTFPrimitives> primitives)
        {
            var it = primitives.GetEnumerator();
            if (!it.MoveNext())
            {
                return false;
            }

            int i = 1;
            var first = it.Current;

            while (it.MoveNext())
            {
                ++i;
                if (!HasSameBuffer(first, it.Current))
                {
                    return false;
                }
            }

            return i>1;
        }
    }

    public class Scene
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        ReactiveProperty<Source> m_source = new ReactiveProperty<Source>();
        public ReadOnlyReactiveProperty<Source> Source { get { return m_source.ToReadOnlyReactiveProperty(); } }
        public IObservable<Source> SourceObservableOnCurrent
        {
            get
            {
                return Source
                        .SkipWhile(x => x.GLTF == null)
                        .ObserveOn(SynchronizationContext.Current);
            }
        }

        public async void Load(string path)
        {
            Logger.Info("----------------------------------------------------------");
            Logger.Info($"Load: { Path.GetFileName(path)}");

            // clear
            m_source.Value = default(Source);

            // async load
            try
            {
                m_source.Value = await Task.Run(() =>
                {
                    return _Load(path);
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"fail to load: {path}: {ex.Message}");
            }
        }

        static ZipArchiveStorage LoadZip(Byte[] fileBytes)
        {
            try
            {
                return ZipArchiveStorage.Parse(fileBytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Source _Load(string path)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            IStorage folder = new FileSystemStorage(System.IO.Path.GetDirectoryName(path));
            var fileBytes = File.ReadAllBytes(path);

            var zip = LoadZip(fileBytes);
            if (zip != null)
            {
                var found = false;
                foreach (var x in zip.Entries)
                {
                    var ext = System.IO.Path.GetExtension(x.FileName).ToLower();
                    if (ext == ".gltf"
                        || ext == ".glb"
                        || ext == ".vrm")
                    {
                        folder = zip;
                        fileBytes = zip.Extract(x);
                        if (fileBytes.Length == 0)
                        {
                            throw new Exception("empty bytes");
                        }
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new Exception("no model file in zip");
                }
            }

            var source = new Source
            {
                Path = path
            };
            try
            {
                // try GLB

                var it = glbImporter.ParseGlbChanks(fileBytes).GetEnumerator();

                if (!it.MoveNext()) throw new FormatException();
                var jsonChunk = it.Current;
                if (jsonChunk.ChunkType != GlbChunkType.JSON)
                {
                    throw new FormatException();
                }

                if (!it.MoveNext()) throw new FormatException();
                var bytesChunk = it.Current;
                if (bytesChunk.ChunkType != GlbChunkType.BIN)
                {
                    throw new FormatException();
                }

                source.JSON = JsonParser.Parse(new Utf8String(jsonChunk.Bytes));
                source.IO = new SimpleStorage(bytesChunk.Bytes);
            }
            catch (Exception)
            {
                // try GLTF
                source.JSON = JsonParser.Parse(new Utf8String(fileBytes));
                source.IO = folder;
            }
            Logger.Info($"Parse: {sw.Elapsed.TotalSeconds} sec");
            sw = System.Diagnostics.Stopwatch.StartNew();

            glTF gltf = null;
            source.JSON.Deserialize(ref gltf);
            source.GLTF = gltf;

            Logger.Info($"Deserialize: {sw.Elapsed.TotalSeconds} sec");

            return source;
        }
    }
}
