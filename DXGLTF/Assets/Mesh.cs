using D3DPanel;
using GltfScene;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DXGLTF.Assets
{
    public class Mesh : IDisposable
    {
        public List<Submesh> Submeshes = new List<Submesh>();

        public void Dispose()
        {
            foreach (var sm in Submeshes)
            {
                sm.Dispose();
            }
            Submeshes.Clear();
        }

        public Mesh()
        {

        }

        public Mesh(Submesh subMesh)
        {
            Submeshes.Add(subMesh);
        }

        public static Mesh FromGLTF(Source source, UniGLTF.glTFMesh m, List<D3D11Material> materials)
        {
            var mesh = new Mesh();

            foreach (var prim in m.primitives)
            {
                mesh.Submeshes.Add(new Submesh
                {
                    Mesh = FromGLTF(source, prim),
                    Material = materials[prim.material],
                });
            }

            return mesh;
        }

        static D3D11Mesh FromGLTF(Source source, UniGLTF.glTFPrimitives primitive)
        {
            var gltf = source.GlTF;
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

            var drawable = new D3D11Mesh(SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                indices);

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

            return drawable;
        }
    }
}
