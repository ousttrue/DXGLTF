using D3DPanel;
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

        List<D3D11Drawable> m_value = new List<D3D11Drawable>();
        public List<D3D11Drawable> Value
        {
            get { return m_value; }
        }

        public void Dispose()
        {
            foreach (var x in m_value)
            {
                x.Dispose();
            }
            m_value.Clear();

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
        public Node(D3D11Drawable drawable)
        {
            m_value.Add(drawable);
        }
    }
}
