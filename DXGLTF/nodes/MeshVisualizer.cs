using D3DPanel;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;


namespace DXGLTF.nodes
{
    class MeshVisualizer : IVisualizer
    {
        public bool BuildNode(GltfScene.Source source, JsonPointer p, ShaderLoader shaderLoader, List<Node> drawables)
        {
            switch (p[0].ToString())
            {
                case "meshes":
                    if (p.Count == 1)
                    {
                        // all meshes
                        ShowMesh(source, source.GlTF.meshes, shaderLoader, drawables);
                    }
                    else
                    {
                        var index = p[1].ToInt32();
                        ShowMesh(source, new[] { source.GlTF.meshes[index] }, shaderLoader, drawables);
                    }
                    return true;

                case "nodes":
                    if (p.Count == 1)
                    {
                        // all nodes
                        ShowNode(source, source.GlTF.nodes, shaderLoader, drawables);
                    }
                    else
                    {
                        var index = p[1].ToInt32();
                        ShowNode(source, new[] { source.GlTF.nodes[index] }, shaderLoader, drawables);
                    }
                    return true;

                default:
                    return false;
            }
        }

        static void ShowNode(GltfScene.Source source, IEnumerable<UniGLTF.glTFNode> nodes,
            ShaderLoader shaderLoader, List<Node> drawables)
        {
            var gltf = source.GlTF;
            foreach (var node in nodes)
            {
                if (node.mesh >= 0)
                {
                    var mesh = gltf.meshes[node.mesh];
                    foreach (var primitive in mesh.primitives)
                    {
                        var drawable = PrimitiveToDrawable(ref source, shaderLoader, gltf, primitive);

                        if (node.matrix != null)
                        {
                            //drawable.Matrix = new Matrix(node.matrix);
                        }
                        else
                        {
                            var t = Vector3.Zero;
                            var r = Quaternion.Identity;
                            var s = Vector3.One;

                            if (node.translation != null)
                            {
                                t.X = node.translation[0];
                                t.Y = node.translation[1];
                                t.Z = node.translation[2];
                            }

                            if (node.rotation != null)
                            {
                                r.X = node.rotation[0];
                                r.Y = node.rotation[1];
                                r.Z = node.rotation[2];
                                r.W = node.rotation[3];
                            }

                            if (node.scale != null)
                            {
                                s.X = node.scale[0];
                                s.Y = node.scale[1];
                                s.Z = node.scale[2];
                            }

                            drawable.Matrix = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, s, Vector3.Zero, r, t);
                            drawable.Matrix.Transpose();
                        }

                        drawables.Add(drawable);
                    }
                }
            }
        }

        static void ShowMesh(GltfScene.Source source, IEnumerable<UniGLTF.glTFMesh> meshes,
            ShaderLoader shaderLoader, List<Node> drawables)
        {
            var gltf = source.GlTF;
            foreach (var mesh in meshes)
            {
                foreach (var primitive in mesh.primitives)
                {
                    var drawable = PrimitiveToDrawable(ref source, shaderLoader, gltf, primitive);
                    drawables.Add(drawable);
                }
            }
        }

        static Node PrimitiveToDrawable(ref GltfScene.Source source, ShaderLoader m_shaderLoader, UniGLTF.glTF gltf, UniGLTF.glTFPrimitives primitive)
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

            return new Node(drawable);
        }
    }
}
