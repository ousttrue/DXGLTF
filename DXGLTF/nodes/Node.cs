using D3DPanel;
using SharpDX;
using System.Collections.Generic;
using UniJSON;


namespace DXGLTF.nodes
{
    class Node : ITreeNode<Node, D3D11Drawable>
    {
        List<Node> m_nodes = new List<Node>();

        public bool IsValid => m_nodes != null;

        int m_parentIndex = -1;
        public int ParentIndex
        {
            get { return m_parentIndex; }
        }

        public bool HasParent => ParentIndex != -1;

        public Node Parent
        {
            get
            {
                if (!HasParent) return null;
                return m_nodes[ParentIndex];
            }
        }

        public IEnumerable<Node> Children
        {
            get
            {
                foreach (var node in m_nodes)
                {
                    if (node.ParentIndex == ValueIndex)
                    {
                        yield return node;
                    }
                }
            }
        }

        public int ValueIndex
        {
            get;
            set;
        }

        D3D11Drawable m_value;
        public D3D11Drawable Value { get { return m_value; } }
        public void SetValue(D3D11Drawable value)
        {
            m_value = value;
        }

        Matrix _matrix = Matrix.Identity;
        public Matrix Matrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;
            }
        }

        public Node(D3D11Drawable value)
        {
            m_value = value;
        }
    }
}
