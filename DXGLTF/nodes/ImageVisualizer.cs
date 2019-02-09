using D3DPanel;
using GltfScene;
using SharpDX;
using System.Collections.Generic;
using UniJSON;


namespace DXGLTF.Nodes
{
    class ImageVisualizer : IVisualizer
    {
        public bool BuildNode(Source source, JsonPointer p, ShaderLoader shaderLoader, List<Node> drawables)
        {
            if (p[0].ToString() != "images")
            {
                return false;
            }
            if (p.Count == 1)
            {
                ShowImage(source, source.GlTF.images, shaderLoader, drawables);
            }
            else
            {
                var index = p[1].ToInt32();
                ShowImage(source, new[] { source.GlTF.images[index] }, shaderLoader, drawables);
            }
            return false;
        }

        static void ShowImage(Source source, IEnumerable<UniGLTF.glTFImage> images,
            ShaderLoader m_shaderLoader, List<Node> m_drawables)
        {
            var gltf = source.GlTF;
            foreach (var image in images)
            {
                var bytes = source.GetImageBytes(image);
                var shader = m_shaderLoader.CreateShader(ShaderType.Unlit);

                var drawable = new D3D11Mesh(SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                    new int[] { 0, 1, 2, 2, 3, 0 });
                drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0),
                }));
                drawable.SetAttribute(Semantics.TEXCOORD, VertexAttribute.Create(new Vector2[]{
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                }));

                m_drawables.Add(new Node());

                break;
            }
        }
    }
}
