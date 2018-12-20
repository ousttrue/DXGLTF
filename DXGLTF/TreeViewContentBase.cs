using GltfScene;
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

            m_scene.SourceObservableOnCurrent.Subscribe(x =>
            {
                OnUpdated(x);
            });
        }
        protected abstract void OnUpdated(Source source);
        protected TreeView TreeView { get { return treeView1; } }
    }

    class NodeContent : TreeViewContentBase
    {
        public NodeContent(Scene scene) : base(scene)
        {
        }

        TreeNode[] m_nodes;
        glTF m_gltf;

        protected override void OnUpdated(Source source)
        {
            TreeView.Nodes.Clear();
            glTF gltf = source.GlTF;
            m_gltf = gltf;
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
    }

    class JsonNodeContent : TreeViewContentBase
    {
        public JsonNodeContent(Scene scene):base(scene)
        {
        }

        protected override void OnUpdated(Source source)
        {
        }
    }
}
