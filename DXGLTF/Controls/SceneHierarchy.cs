using D3DPanel;
using DXGLTF.Assets;
using DXGLTF.Nodes;
using GltfScene;
using NLog;
using Reactive.Bindings;
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

        ReactiveProperty<Node> _selected = new ReactiveProperty<Node>();
        public ReadOnlyReactiveProperty<Node> Selected
        {
            get { return _selected.ToReadOnlyReactiveProperty(); }
        }

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
        Mesh _manipulator;
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

            if (_manipulator != null)
            {
                _manipulator.Dispose();
                _manipulator = null;
            }
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

            // manipulator
            {
                var manipulator = new D3D11Material(_shaderLoader.CreateShader(ShaderType.Gizmo),
                    false, default(ImageBytes), Color.White);
                var radius = 0.005f;
                var length = 0.3f;
                _manipulator = new Mesh(
                    new Submesh(manipulator, D3D11MeshFactory.CreateArrow(radius, length, 0, true, new Color4(1, 0, 0, 1)))
                    , new Submesh(manipulator, D3D11MeshFactory.CreateArrow(radius, length, 0, false, new Color4(0.5f, 0, 0, 1)))
                    , new Submesh(manipulator, D3D11MeshFactory.CreateArrow(radius, length, 1, true, new Color4(0, 1, 0, 1)))
                    , new Submesh(manipulator, D3D11MeshFactory.CreateArrow(radius, length, 1, false, new Color4(0, 0.5f, 0, 1)))
                    , new Submesh(manipulator, D3D11MeshFactory.CreateArrow(radius, length, 2, true, new Color4(0, 0, 1.0f, 1)))
                    , new Submesh(manipulator, D3D11MeshFactory.CreateArrow(radius, length, 2, false, new Color4(0, 0, 0.5f, 1)))
                    );
            }
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

        void SetTreeNode(AssetContext asset)
        {
            TreeView.Nodes.Clear();
            ClearDrawables();
            _map.Clear();
            _selected.Value = null;
            if (asset == null)
            {
                return;
            }

            var nodes = asset.BuildHierarchy();

            var roots = nodes.Where(x => !nodes.Any(y => y.Children.Contains(x)));

            // treeview
            foreach (var root in roots)
            {
                Traverse(TreeView.Nodes, root);
            }

            // drawables
            _drawables.AddRange(roots);

            _updated.OnNext(Unit.Default);
        }

        protected override void OnSelected(TreeNode viewNode)
        {
            var node = default(Node);
            if (!_map.TryGetValue(viewNode, out node))
            {
                Logger.Warn($"{viewNode} not found");
            }
            _selected.Value = node;
            _updated.OnNext(Unit.Default);
        }

        public void Intersect(Ray ray)
        {
            if (Selected.Value == null)
            {
                return;
            }
            Logger.Debug(ray);

            foreach(var t in _manipulator.Intersect(Selected.Value.WorldMatrix, ray))
            {
                Logger.Debug($"Intersect {t}");
            }
        }

        public void Draw(D3D11Renderer renderer, Camera camera)
        {
            foreach (var node in _gizmos)
            {
                DrawNode(renderer, camera, node, Matrix.Identity);
            }

            // clear depth
            //_renderer.ClearDepth();

            foreach (var node in _drawables)
            {
                DrawNode(renderer, camera, node, Matrix.Identity);
            }

            if (Selected.Value != null)
            {
                DrawMesh(renderer, camera, _manipulator, Selected.Value.WorldMatrix);
            }
        }

        static void DrawNode(D3D11Renderer renderer, Camera camera, Node node, Matrix accumulated)
        {
            node.WorldMatrix = node.LocalMatrix * accumulated;
            
            DrawMesh(renderer, camera, node.Mesh, node.WorldMatrix);

            foreach (var child in node.Children)
            {
                DrawNode(renderer, camera, child, node.WorldMatrix);
            }
        }

        static void DrawMesh(D3D11Renderer renderer, Camera camera, Mesh mesh, Matrix m)
        {
            if (mesh == null)
            {
                return;
            }

            foreach (var x in mesh.Submeshes)
            {
                renderer.Draw(camera, x.Material, x.Mesh, m);
            }
        }
    }
}
