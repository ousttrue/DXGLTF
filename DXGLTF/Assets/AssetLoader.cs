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


namespace DXGLTF.Assets
{
    public class AssetLoader
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        ReactiveProperty<AssetSource> m_source = new ReactiveProperty<AssetSource>();
        public ReadOnlyReactiveProperty<AssetSource> Source { get { return m_source.ToReadOnlyReactiveProperty(); } }
        public IObservable<AssetSource> SourceObservableOnCurrent
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
            m_source.Value = default(AssetSource);

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

        public static AssetSource _Load(string path)
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

            var source = new AssetSource
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
