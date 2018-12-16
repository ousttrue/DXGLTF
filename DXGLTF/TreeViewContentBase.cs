﻿using GltfScene;
using System;
using System.Windows.Forms;
using System.Linq;
using UniGLTF;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public abstract partial class TreeViewContentBase : DockContent
    {
        Scene m_scene;
        public TreeViewContentBase(Scene scene)
        {
            m_scene = scene;

            InitializeComponent();

            m_scene.GltfObservableOnCurrent.Subscribe(OnUpdated);
        }
        protected abstract void OnUpdated(glTF gltf);
        protected TreeView TreeView { get { return treeView1; } }
    }

    class NodeContent : TreeViewContentBase
    {
        public NodeContent(Scene scene):base(scene)
        {
        }

        TreeNode[] m_nodes;

        protected override void OnUpdated(glTF gltf)
        {
            TreeView.Nodes.Clear();

            m_nodes = gltf.nodes.Select(x => new TreeNode(x.name)).ToArray();

            for(int i=0; i<gltf.nodes.Count; ++i)
            {
                var parent = m_nodes[i];

                var node = gltf.nodes[i];
                if (node.children != null) {
                    foreach (var j in node.children)
                    {
                        parent.Nodes.Add(m_nodes[j]);
                    }
                }
            }

            TreeView.Nodes.Add(m_nodes[0]);
            TreeView.ExpandAll();
        }
    }
}
