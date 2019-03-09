using DXGLTF.Assets;
using NLog;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DXGLTF
{
    public class SceneHierarchyContent : TreeViewContentBase
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        Scene _scene;

        public SceneHierarchyContent(Scene scene)
        {
            _scene = scene;
        }

        Dictionary<TreeNode, Node> _map = new Dictionary<TreeNode, Node>();

        void Traverse(TreeNodeCollection parent, Node node)
        {
            var viewNode = new TreeNode(node.Name);
            parent.Add(viewNode);
            _map.Add(viewNode, node);
            foreach (var child in node.Children)
            {
                Traverse(viewNode.Nodes, child);
            }
        }

        public void SetTreeNode(AssetContext asset)
        {
            TreeView.Nodes.Clear();
            _map.Clear();

            if (asset != null)
            {
                // treeview
                foreach (var root in asset.Roots)
                {
                    Traverse(TreeView.Nodes, root);
                }
            }
        }

        protected override void OnSelected(TreeNode viewNode)
        {
            var node = default(Node);
            if (!_map.TryGetValue(viewNode, out node))
            {
                Logger.Warn($"{viewNode} not found");
            }

            _scene.Selected = node;
        }
    }
}
