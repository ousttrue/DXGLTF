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
        ReactiveProperty<string> m_json = new ReactiveProperty<string>();
        public ReactiveProperty<string> Json { get { return m_json; } }

        ReactiveProperty<glTF> m_gltf = new ReactiveProperty<glTF>();
        public ReactiveProperty<glTF> Gltf { get { return m_gltf; } }

        public IObservable<glTF> GltfObservableOnCurrent
        {
            get
            {
                return Gltf
                        .ObserveOn(SynchronizationContext.Current);

            }
        }

        public async void Load(string path)
        {
            Json.Value = "";
            Gltf.Value = null;

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
            Gltf.Value = gltf;
           
            Console.WriteLine(Json);
        }

        public void LoadGlb(string path)
        {
            var bytes = File.ReadAllBytes(path);
        }
    }
}
