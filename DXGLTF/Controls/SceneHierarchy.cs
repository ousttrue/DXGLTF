using D3DPanel;
using DXGLTF.Assets;
using DXGLTF.Nodes;
using GltfScene;
using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DXGLTF
{
    public class SceneHierarchy : TreeViewContentBase
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        ShaderLoader _shaderLoader = new ShaderLoader();

        Assets.AssetContext _asset;
        Assets.AssetContext Asset
        {
            get { return _asset; }
            set
            {
                if (_asset == value) return;

                if (_asset != null)
                {
                    _asset.Dispose();
                }
                _asset = value;

                SetTreeNode(value);
            }
        }

        List<Node> _gizmos = new List<Node>();
        List<Node> _drawables = new List<Node>();
        void ClearDrawables()
        {
            foreach (var x in _drawables)
            {
                x.Dispose();
            }
            _drawables.Clear();
        }

        public void Shutdown()
        {
            ClearDrawables();

            foreach (var x in _gizmos)
            {
                x.Dispose();
            }
            _gizmos.Clear();
        }

        Subject<Unit> _updated = new Subject<Unit>();
        public IObservable<Unit> Updated
        {
            get { return _updated.AsObservable(); }
        }

        public SceneHierarchy(Scene scene) : base(scene)
        {
            var unlit = _shaderLoader.CreateShader(ShaderType.Unlit);
            var gizmo = _shaderLoader.CreateShader(ShaderType.Gizmo);

            // default triangle
            _drawables.Add(new Node(gizmo, D3D11MeshFactory.CreateTriangle()));

            // gizmos
            _gizmos.Add(new Node(gizmo, D3D11MeshFactory.CreateAxis(0.1f, 10.0f)));
            _gizmos.Add(new Node(gizmo, D3D11MeshFactory.CreateGrid(1.0f, 10)));
        }

        protected override void OnUpdated(Source source)
        {
            LoadAsset(source);
        }

        async void LoadAsset(Source source)
        {
            if (source.GlTF == null)
            {
                Asset = null;
                return;
            }

            Asset = await Task.Run(() => AssetContext.Load(source, _shaderLoader));
        }

        void Traverse(TreeNodeCollection parent, Node node)
        {
            var viewNode = new TreeNode(node.Name);
            parent.Add(viewNode);
            foreach(var child in node.Children)
            {
                Traverse(viewNode.Nodes, child);
            }
        }

        void SetTreeNode(AssetContext asset)
        {
            TreeView.Nodes.Clear();
            ClearDrawables();
            if (asset == null)
            {
                return;
            }

            var nodes  = asset.BuildHierarchy();

            var roots = nodes.Where(x => !nodes.Any(y => y.Children.Contains(x)));

            // treeview
            foreach(var root in roots)
            {
                Traverse(TreeView.Nodes, root);
            }

            // drawables
            _drawables.AddRange(roots);

            _updated.OnNext(Unit.Default);
        }

        protected override void OnSelected(TreeNode node)
        {
        }

        public void Draw(D3D11Renderer renderer, Camera camera)
        {
            foreach (var node in _gizmos)
            {
                RendererDraw(renderer, camera, node, Matrix.Identity);
            }

            // clear depth
            //_renderer.ClearDepth();

            foreach (var node in _drawables)
            {
                RendererDraw(renderer, camera, node, Matrix.Identity);
            }
        }

        void RendererDraw(D3D11Renderer renderer, Camera camera, Node node, Matrix accumulated)
        {
            var m = node.LocalMatrix * accumulated;
            //Logger.Debug(m);
            if (node.Mesh != null)
            {
                foreach (var x in node.Mesh.Submeshes)
                {
                    renderer.Draw(camera, x.Material, x.Mesh, m);
                }
            }

            foreach (var child in node.Children)
            {
                RendererDraw(renderer, camera, child, m);
            }
        }
    }
}
