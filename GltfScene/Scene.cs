using Reactive.Bindings;
using System;
using System.IO;
using UniJSON;
using UniGLTF;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;

namespace GltfScene
{
    public struct Source
    {
        public IBufferIO IO;
        public glTF GlTF;
        public JsonNode JSON;
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
                        .SkipWhile(x => x.GlTF==null)
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
            catch(Exception ex)
            {
                Logger.Error($"fail to load: {path}: {ex}");
            }
        }

        static Source _Load(string path)
        {
            var bytes = File.ReadAllBytes(path);

            Source source;

            try
            {
                // try as GLB

                var it = glbImporter.ParseGlbChanks(bytes).GetEnumerator();

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
                source.IO = new BytesIO(bytesChunk.Bytes);
            }
            catch (Exception ex)
            {
                // try as GLTF
                source.JSON = JsonParser.Parse(new Utf8String(bytes));
                source.IO = FolderIO.FromFile(path);
            }

            glTF gltf = null;
            source.JSON.Deserialize(ref gltf);
            source.GlTF = gltf;

            return source;
        }
    }
}
