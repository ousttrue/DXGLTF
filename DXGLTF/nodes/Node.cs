using D3DPanel;
using DXGLTF.Assets;
using SharpDX;
using System;
using System.Collections.Generic;
using UniJSON;


namespace DXGLTF.Nodes
{
    public class Node : IDisposable
    {
        public bool IsValid => true;

        public List<Node> Children = new List<Node>();

        public Mesh Mesh
        {
            get;
            set;
        }

        public void Dispose()
        {
            if (Mesh != null)
            {
                Mesh.Dispose();
                Mesh = null;
            }

            foreach (var x in Children)
            {
                x.Dispose();
            }
            Children.Clear();
        }

        Matrix _matrix = Matrix.Identity;
        public Matrix LocalMatrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;
            }
        }

        public Matrix WorldMatrix
        {
            get;
            set;
        }

        public string Name
        {
            get;
            private set;
        }

        public Node(string name)
        {
            Name = name;
        }

        public Node(D3D11Shader shader, D3D11Mesh mesh)
        {
            Mesh = new Mesh(new Submesh(shader, mesh));
        }

        public struct SubmeshIntersection
        {
            public int SubmeshIndex;
            public TriangleIntersection Triangle;
        }

        public IEnumerable<SubmeshIntersection> Intersect(Ray ray)
        {
            // transform the picking ray into the object space of the mesh
            var invWorld = Matrix.Invert(WorldMatrix);
            ray.Direction = Vector3.TransformNormal(ray.Direction, invWorld);
            ray.Position = Vector3.TransformCoordinate(ray.Position, invWorld);
            ray.Direction.Normalize();

            if (Mesh != null)
            {
                for(int i=0; i<Mesh.Submeshes.Count; ++i)
                {
                    foreach(var intersection in Mesh.Submeshes[i].Mesh.Intersect(ray))
                    {
                        yield return new SubmeshIntersection
                        {
                            SubmeshIndex = i,
                            Triangle = intersection
                        };
                    }
                }
            }
        }
    }
}
