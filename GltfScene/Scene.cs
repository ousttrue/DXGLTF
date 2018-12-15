using Reactive.Bindings;
using System;
using System.IO;
using UniJSON;


namespace GltfScene
{
    public class Scene
    {
        ReactiveProperty<string> m_json;
        public ReactiveProperty<string> Json
        {
            get
            {
                if (m_json == null)
                {
                    m_json = new ReactiveProperty<string>();
                }
                return m_json;
            }
        }

        public void Load(string path)
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

            Console.WriteLine(Json);
        }

        public void LoadGlb(string path)
        {
            var bytes = File.ReadAllBytes(path);
        }
    }
}
