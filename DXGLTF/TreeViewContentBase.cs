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

    class NodeContent : TreeViewContentBase
    {
        public NodeContent(Scene scene) : base(scene)
        {
        }

        TreeNode[] m_nodes;

        protected override void OnUpdated(Source source)
        {
            TreeView.Nodes.Clear();
            glTF gltf = source.GlTF;
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

    class JsonNodeContent : TreeViewContentBase
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        ReactiveProperty<ListTreeNode<JsonValue>> m_selected = new ReactiveProperty<ListTreeNode<JsonValue>>();
        public ReadOnlyReactiveProperty<ListTreeNode<JsonValue>> Selected
        {
            get { return m_selected.ToReadOnlyReactiveProperty(); }
        }

        public JsonNodeContent(Scene scene) : base(scene)
        {
        }

        Dictionary<TreeNode, ListTreeNode<JsonValue>> m_nodeMap = new Dictionary<TreeNode, ListTreeNode<JsonValue>>();
        void Traverse(TreeNodeCollection parent, string key, ListTreeNode<JsonValue> node)
        {
            if (node.IsArray())
            {
                var current = new TreeNode($"{key}({node.GetArrayCount()})");
                parent.Add(current);
                m_nodeMap.Add(current, node);

                int i = 0;
                foreach (var x in node.ArrayItems())
                {
                    Traverse(current.Nodes, (i++).ToString(), x);
                }
            }
            else if (node.IsMap())
            {
                var current = new TreeNode(key);
                parent.Add(current);
                m_nodeMap.Add(current, node);

                foreach (var kv in node.ObjectItems())
                {
                    Traverse(current.Nodes, kv.Key.GetString(), kv.Value);
                }
            }
            else
            {
                var current = new TreeNode($"{key}: {node.ToString()}");
                parent.Add(current);
                m_nodeMap.Add(current, node);
            }
        }

        Source m_source;
        public Source Source
        {
            get { return m_source; }
        }

        protected override void OnUpdated(Source source)
        {
            // clear
            m_nodeMap.Clear();
            TreeView.Nodes.Clear();
            m_source = source;

            glTF gltf = source.GlTF;
            if (gltf == null)
            {
                return;
            }

            Traverse(TreeView.Nodes, "GLTF", source.JSON);

            foreach (TreeNode x in TreeView.Nodes)
            {
                x.Expand();
            }
        }

        protected override void OnSelected(TreeNode node)
        {
            var json = default(ListTreeNode<JsonValue>);
            if (!m_nodeMap.TryGetValue(node, out json))
            {
                Logger.Warn($"{node} not found");
            }
            m_selected.Value = json;
        }
    }
}
