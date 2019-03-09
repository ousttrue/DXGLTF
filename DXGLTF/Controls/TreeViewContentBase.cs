using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public abstract partial class TreeViewContentBase : DockContent
    {
        protected TreeView TreeView { get { return treeView1; } }

        protected TreeViewContentBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ツリーノードが選択された
        /// </summary>
        /// <param name="node"></param>
        protected abstract void OnSelected(TreeNode node);

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnSelected(treeView1.SelectedNode);
        }
    }
}
