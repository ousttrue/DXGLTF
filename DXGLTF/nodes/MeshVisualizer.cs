using D3DPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;


namespace DXGLTF.nodes
{
    class MeshVisualizer : IVisualizer
    {
        public bool BuildNode(GltfScene.Source source, JsonPointer p, ShaderLoader shaderLoader, List<Node> nodes)
        {
            if (p[0].ToString() != "meshes")
            {
                return false;
            }
            if (p.Count == 1)
            {
                ShowMesh(source, source.GlTF.meshes, shaderLoader, nodes);
            }
            else
            {
                var index = p[1].ToInt32();
                ShowMesh(source, new[] { source.GlTF.meshes[index] }, shaderLoader, nodes);
            }
            return true;
        }

        static void ShowMesh(GltfScene.Source source, IEnumerable<UniGLTF.glTFMesh> meshes,
            ShaderLoader m_shaderLoader, List<Node> drawables)
        {
            var gltf = source.GlTF;
            foreach (var mesh in meshes)
            {
                foreach (var primitive in mesh.primitives)
                {
                    var m = gltf.materials[primitive.material];

                    var imageBytes = default(ImageBytes);
                    if (m.pbrMetallicRoughness != null)
                    {
                        var colorTexture = m.pbrMetallicRoughness.baseColorTexture;
                        if (colorTexture != null)
                        {
                            if (colorTexture.index != -1)
                            {
                                var texture = gltf.textures[colorTexture.index];
                                var image = gltf.images[texture.source];
                                var bytes = source.GetImageBytes(image);
                                imageBytes = new ImageBytes(bytes);
                            }
                        }
                    }
                    var material = m_shaderLoader.CreateMaterial(ShaderType.Unlit,
                        imageBytes);
                    var accessor = gltf.accessors[primitive.indices];
                    int[] indices = null;
                    switch (accessor.componentType)
                    {
                        case UniGLTF.glComponentType.BYTE:
                            indices = gltf.GetArrayFromAccessor<byte>(source.IO, primitive.indices).Select(x => (int)x).ToArray();
                            break;

                        case UniGLTF.glComponentType.UNSIGNED_SHORT:
                            indices = gltf.GetArrayFromAccessor<ushort>(source.IO, primitive.indices).Select(x => (int)x).ToArray();
                            break;

                        case UniGLTF.glComponentType.UNSIGNED_INT:
                            indices = gltf.GetArrayFromAccessor<int>(source.IO, primitive.indices);
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    var drawable = new D3D11Drawable(indices, material);

                    var attribs = primitive.attributes;

                    {
                        var positions = gltf.GetBytesFromAccessor(source.IO, primitive.attributes.POSITION);
                        if (positions.Count == 0)
                        {
                            throw new Exception();
                        }
                        drawable.SetAttribute(Semantics.POSITION, new VertexAttribute(positions, 4 * 3));
                    }

                    if (primitive.attributes.TEXCOORD_0 != -1)
                    {
                        var uv = gltf.GetBytesFromAccessor(source.IO, primitive.attributes.TEXCOORD_0);
                        drawable.SetAttribute(Semantics.TEXCOORD, new VertexAttribute(uv, 4 * 2));
                    }

                    drawables.Add(new Node(drawable));
                }
            }
        }
    }
}
