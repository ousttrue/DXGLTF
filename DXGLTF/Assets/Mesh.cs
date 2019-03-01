using D3DPanel;
using GltfScene;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DXGLTF.Assets
{
    public struct SubmeshIntersection
    {
        public int SubmeshIndex;
        public TriangleIntersection Triangle;

        public override string ToString()
        {
            return $"[{SubmeshIndex}]";
        }
    }

    public class Skin
    {
        public int RootIndex;
        public int[] Joints;
        public Matrix[] BindMatrices;
        public static Skin FromGLTF(Source source, UniGLTF.glTFSkin skin)
        {
            return new Skin
            {
                RootIndex = skin.skeleton,
                Joints = skin.joints,
                BindMatrices = source.GLTF.GetArrayFromAccessor<Matrix>(source.IO, skin.inverseBindMatrices)
            };
        }
    }

    public class Mesh : IDisposable
    {
        Skin _skin;
        Node[] _bindNodes;

        public void SetSkin(Skin skin, Node[] nodes)
        {
            _skin = skin;
            _bindNodes = skin.Joints.Select(x => nodes[x]).ToArray();
        }

        public List<Submesh> Submeshes = new List<Submesh>();

        public IEnumerable<SubmeshIntersection> Intersect(Matrix world, Ray ray)
        {
            // transform the picking ray into the object space of the mesh
            var invWorld = Matrix.Invert(world);
            ray.Direction = Vector3.TransformNormal(ray.Direction, invWorld);
            ray.Position = Vector3.TransformCoordinate(ray.Position, invWorld);
            ray.Direction.Normalize();

            for (int i = 0; i < Submeshes.Count; ++i)
            {
                foreach (var intersection in Submeshes[i].Mesh.Intersect(ray))
                {
                    yield return new SubmeshIntersection
                    {
                        SubmeshIndex = i,
                        Triangle = intersection
                    };
                }
            }
        }

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

        public Mesh(params Submesh[] subMesh)
        {
            Submeshes.AddRange(subMesh);
        }

        public static Mesh FromGLTF(Source source,
            UniGLTF.glTFMesh m, List<D3D11Material> materials)
        {
            var mesh = new Mesh();
            mesh.Submeshes.AddRange(FromGLTF(source, m.primitives, materials));
            return mesh;
        }

        public static IEnumerable<Submesh> FromGLTF(Source source,
            List<UniGLTF.glTFPrimitives> primitives, List<D3D11Material> materials)
        {
            if (source.HasSameBuffer(primitives))
            {
                var mesh = FromGLTF(source, primitives[0]);
                foreach (var prim in primitives)
                {
                    var indices = source.GLTF.accessors[prim.indices];
                    var offset = indices.byteOffset / indices.ElementSize;
                    yield return new Submesh(materials[prim.material],
                        mesh, offset, indices.count);
                }
            }
            else
            {
                foreach (var prim in primitives)
                {
                    yield return new Submesh(materials[prim.material],
                        FromGLTF(source, prim));
                }
            }
        }

        static D3D11Mesh FromGLTF(Source source,
            UniGLTF.glTFPrimitives primitive)
        {
            var gltf = source.GLTF;
            var accessor = gltf.accessors[primitive.indices];
            int[] indices = null;
            switch (accessor.componentType)
            {
                case UniGLTF.glComponentType.UNSIGNED_BYTE:
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

        public void Draw(D3D11Renderer renderer, Camera camera, Matrix m)
        {
            foreach (var submesh in Submeshes)
            {
                submesh.Draw(renderer, camera, submesh.Material, submesh.Mesh, m);
            }
        }
    }
}
