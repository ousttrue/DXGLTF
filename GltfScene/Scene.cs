using System;
using System.IO;
using UniJSON;


namespace GltfScene
{
    public class Scene
    {
        public string Json
        {
            get;
            private set;
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

            Json = parsed.ToString();

            Console.WriteLine(Json);
        }

        public void LoadGlb(string path)
        {
            var bytes = File.ReadAllBytes(path);
        }
    }
}
