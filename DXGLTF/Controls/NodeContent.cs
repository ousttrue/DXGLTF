﻿using GltfScene;
using System.Linq;
using System.Windows.Forms;


namespace DXGLTF
{
    class NodeContent : TreeViewContentBase
    {
        public NodeContent(Scene scene) : base(scene)
        {
        }

        TreeNode[] m_nodes;

        protected override void OnUpdated(Source source)
        {
            TreeView.Nodes.Clear();
            var gltf = source.GlTF;
            if (gltf == null)
            {
                return;
            }

            m_nodes = gltf.nodes.Select((x, i) => new TreeNode(string.Format("[{0}]{1}", i, x.name))).ToArray();

            for (int i = 0; i < gltf.nodes.Count; ++i)
            {
                var parent = m_nodes[i];

                var node = gltf.nodes[i];
                if (node.children != null)
                {
                    foreach (var j in node.children)
                    {
                        parent.Nodes.Add(m_nodes[j]);
                    }
                }
            }

            TreeView.Nodes.Add(m_nodes[0]);
            TreeView.ExpandAll();
        }

        protected override void OnSelected(TreeNode node)
        {
            //throw new NotImplementedException();
        }
    }
}