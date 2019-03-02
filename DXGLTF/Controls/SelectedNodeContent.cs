using DXGLTF.Assets;
using SharpDX;
using System;
using System.Reactive.Linq;
using System.Threading;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class SelectedNodeContent : DockContent
    {
        public SelectedNodeContent(SceneHierarchy hierarchy)
        {
            InitializeComponent();

            hierarchy.SelectedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(SetNode)
                ;
        }

        class NodeProps
        {
            Node _node;

            public Vector3 T
            {
                get { return _node.LocalPosition; }
                set { _node.LocalPosition = value; }
            }

            public Vector3 R
            {
                get { return _node.LocalEuler; }
                set { _node.LocalEuler = value; }
            }

            public Vector3 S
            {
                get { return _node.LocalScale; }
                set { _node.LocalScale = value; }
            }

            public NodeProps(Node node)
            {
                _node = node;
            }
        }

        void SetNode(Node node)
        {
            if (node == null)
            {
                propertyGrid1.SelectedObject = null;
            }
            else
            {
                propertyGrid1.SelectedObject = new NodeProps(node);
            }
        }
    }
}
