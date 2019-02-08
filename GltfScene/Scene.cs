using NLog;
using Reactive.Bindings;
using System;
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
        public glTF GlTF;
        public ListTreeNode<JsonValue> JSON;

        public ArraySegment<byte> GetImageBytes(glTFImage image)
        {
            if (string.IsNullOrEmpty(image.uri))
            {
                return GlTF.GetViewBytes(IO, image.bufferView);
            }
            else
            {
                return IO.Get(image.uri);
            }
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
                        .SkipWhile(x => x.GlTF == null)
                        .ObserveOn(SynchronizationContext.Current);
            }
        }

        public async void Load(string path)
        {
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

        static void LoadZip(ref IStorage folder, ref Byte[] fileBytes)
        {
            var zip = default(ZipArchiveStorage);
            try
            {
                zip = ZipArchiveStorage.Parse(fileBytes);
            }
            catch (Exception)
            {
                return;
            }
            foreach (var x in zip.Entries)
            {
                var ext = System.IO.Path.GetExtension(x.FileName).ToLower();
                switch (ext)
                {
                    case ".gltf":
                    case ".glb":
                    case ".vrm":
                        folder = zip;
                        fileBytes = zip.Extract(x);
                        return;
                }
            }

            throw new Exception("no model file in zip");
        }

        public static Source _Load(string path)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            IStorage folder = new FileSystemStorage(System.IO.Path.GetDirectoryName(path));
            var fileBytes = File.ReadAllBytes(path);

            LoadZip(ref folder, ref fileBytes);

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

            glTF gltf = null;
            source.JSON.Deserialize(ref gltf);
            source.GlTF = gltf;

            Logger.Info($"OpenFile: parse {Path.GetFileName(path)} in {sw.Elapsed.TotalSeconds} sec");

            return source;
        }
    }
}
