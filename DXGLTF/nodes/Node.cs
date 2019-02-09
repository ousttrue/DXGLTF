using D3DPanel;
using DXGLTF.Assets;
using SharpDX;
using System;
using System.Collections.Generic;
using UniJSON;


namespace DXGLTF.Nodes
{
    class Node : IDisposable
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

        public Node()
        {
        }
    }
}
