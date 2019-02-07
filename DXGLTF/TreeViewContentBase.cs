using GltfScene;
using System;
using System.Windows.Forms;
using System.Linq;
using UniGLTF;
using WeifenLuo.WinFormsUI.Docking;
using UniJSON;
using NLog;
using System.Collections.Generic;
using Reactive.Bindings;

namespace DXGLTF
{
    public abstract partial class TreeViewContentBase : DockContent
    {
        Scene m_scene;
        public TreeViewContentBase(Scene scene)
        {
            m_scene = scene;

            InitializeComponent();

            m_scene.SourceObservableOnCurrent.Subscribe(x =>
            {
                OnUpdated(x);
            });
        }
        protected abstract void OnUpdated(Source source);
        protected TreeView TreeView { get { return treeView1; } }

        protected abstract void OnSelected(TreeNode node);
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnSelected(treeView1.SelectedNode);
        }
    }
}
