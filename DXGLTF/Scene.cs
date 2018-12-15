using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniJSON;


namespace DXGLTF
{
    class Scene
    {
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

            Console.WriteLine(parsed["nodes"].ValueCount);
        }

        public void LoadGlb(string path)
        {
            var bytes = File.ReadAllBytes(path);
        }
    }
}
