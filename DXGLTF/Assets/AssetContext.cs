using D3DPanel;
using GltfScene;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DXGLTF.Assets
{
    class AssetContext : IDisposable
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        List<ImageBytes> _textureImages = new List<ImageBytes>();
        List<D3D11Material> _materials = new List<D3D11Material>();
        List<Mesh> _meshes = new List<Mesh>();

        public void Dispose()
        {
        }

        public static AssetContext Load(Source source, ShaderLoader shaderLoader)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var asset = new AssetContext();

            var gltf = source.GlTF;

            foreach(var texture in gltf.textures)
            {
                var image = gltf.images[texture.source];
                var bytes = source.GetImageBytes(image);
                asset._textureImages.Add(new ImageBytes(bytes));
            }

            foreach(var material in gltf.materials)
            {
                var shader = material.IsUnlit
                    ? shaderLoader.CreateShader(ShaderType.Unlit)
                    : shaderLoader.CreateShader(ShaderType.Standard)
                    ;

                var texture = default(ImageBytes);
                //asset._textureImages
                var color = SharpDX.Color4.White;
                asset._materials.Add(new D3D11Material(shader, texture, color));
            }

            foreach(var mesh in gltf.meshes)
            {
                asset._meshes.Add(Mesh.FromGLTF(source, mesh, asset._materials));
            }

            Logger.Info($"LoadAsset: {sw.Elapsed.TotalSeconds} sec");

            return asset;
        }
    }
}
