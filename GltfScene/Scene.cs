using Reactive.Bindings;
using System;
using System.IO;
using UniJSON;
using UniGLTF;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GltfScene
{
    public class Scene
    {
        ReactiveProperty<string> m_loadPath = new ReactiveProperty<string>();
        public ReactiveProperty<string> LoadPath { get { return m_loadPath; } }

        ReactiveProperty<string> m_json = new ReactiveProperty<string>();
        public ReactiveProperty<string> Json { get { return m_json; } }

        ReactiveProperty<(glTF, IBufferIO)> m_gltf = new ReactiveProperty<(glTF, IBufferIO)>();
        public ReactiveProperty<(glTF, IBufferIO)> Gltf { get { return m_gltf; } }

        public IObservable<(glTF, IBufferIO)> GltfObservableOnCurrent
        {
            get
            {
                return Gltf
                        .SkipWhile(x => x.Item1==null)
                        .ObserveOn(SynchronizationContext.Current);
            }
        }

        public async void Load(string path)
        {
            // clear
            Json.Value = "";
            Gltf.Value = (null, null);

            await Task.Run(() =>
            {
                _Load(path);
            });
        }

        void _Load(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".gltf":
                    LoadGltf(path);
                    break;

                case ".glb":
                    LoadGlb(path);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void LoadGltf(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var parsed = JsonParser.Parse(new Utf8String(bytes));

            Json.Value = parsed.ToString();

            glTF gltf = null;
            parsed.Deserialize(ref gltf);

            LoadPath.Value = path;
            Gltf.Value = (gltf, FolderIO.FromFile(path));
        }

        public void LoadGlb(string path)
        {
            var bytes = File.ReadAllBytes(path);

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

            var parsed = JsonParser.Parse(new Utf8String(jsonChunk.Bytes));

            Json.Value = parsed.ToString();

            glTF gltf = null;
            parsed.Deserialize(ref gltf);

            LoadPath.Value = path;
            Gltf.Value = (gltf, new BytesIO(bytesChunk.Bytes));
        }
    }
}
