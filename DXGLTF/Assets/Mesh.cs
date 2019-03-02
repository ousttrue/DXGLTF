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
        public Matrix[] BindMatrices;
        int[] _joints;

        Matrix[] _nodeMatrices;
        public Matrix[] NodeMatrices
        {
            get
            {
                return _nodeMatrices;
            }
        }

        public void Update(Node[] nodes)
        {
            if (_nodeMatrices == null)
            {
                _nodeMatrices = new Matrix[_joints.Length];
            }

            for (int i = 0; i < _joints.Length; ++i)
            {
                _nodeMatrices[i] = nodes[_joints[i]].WorldMatrix;
            }
        }

        public static Skin FromGLTF(Source source, UniGLTF.glTFSkin skin)
        {
            return new Skin
            {
                RootIndex = skin.skeleton,
                BindMatrices = source.GLTF.GetArrayFromAccessor<Matrix>(source.IO, skin.inverseBindMatrices),
                _joints = skin.joints,
            };
        }
    }

    public class Mesh : IDisposable
    {
        Skin _skin;

        public void SetSkin(Skin skin, Node[] nodes)
        {
            _skin = skin;
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
            if (Submeshes.Count == 0)
            {
                return;
            }

            // world constants
            var mvp = m * camera.View * camera.Projection;
            mvp.Transpose();
            renderer.UpdateWorldConstants(mvp);

            if (Submeshes[0].DrawIndexCount > 0)
            {
                // shared indices
                var first = Submeshes[0];

                if (renderer.SetVertices(first.Material.Shader, first.Mesh))
                {
                    if (renderer.SetIndices(first.Mesh))
                    {
                        foreach (var submesh in Submeshes)
                        {
                            // material constants
                            renderer.SetMaterial(submesh.Material);

                            renderer.DrawIndexed(submesh.DrawIndexOffset, submesh.DrawIndexCount);
                        }
                    }
                }
            }
            else
            {
                foreach (var submesh in Submeshes)
                {
                    if (renderer.SetVertices(submesh.Material.Shader, submesh.Mesh))
                    {
                        // material constants
                        renderer.SetMaterial(submesh.Material);

                        if (renderer.SetIndices(submesh.Mesh))
                        {
                            renderer.DrawIndexed(0, submesh.Mesh.IndexCount);
                        }
                        else
                        {
                            renderer.Draw(0, submesh.Mesh.VertexCount);
                        }
                    }
                    else
                    {
                        int a = 0;
                    }
                }
            }
        }
    }
}
