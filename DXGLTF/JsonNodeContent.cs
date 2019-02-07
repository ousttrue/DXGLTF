using GltfScene;
using NLog;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Windows.Forms;
using UniJSON;


namespace DXGLTF
{
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

            var gltf = source.GlTF;
            if (gltf == null)
            {
                return;
            }

            foreach (var kv in source.JSON.ObjectItems())
            {
                Traverse(TreeView.Nodes, kv.Key.GetString(), kv.Value);
            }

            TreeView.SelectedNode = null;
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
