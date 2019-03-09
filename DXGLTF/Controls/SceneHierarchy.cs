using D3DPanel;
using DXGLTF.Assets;
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
        public Node Selected
        {
            get { return _selected.Value; }
        }
        public IObservable<Node> SelectedObservable
        {
            get { return _selected.AsObservable(); }
        }

        ShaderLoader _shaderLoader = new ShaderLoader();

        AssetContext _asset;
        AssetContext Asset
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

        Mesh _cursor;
        Vector3 _cursorPosition;
        int _index = -1;

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
            var nodepth = new D3D11Material("manipulator", _shaderLoader.CreateShader(ShaderType.Gizmo),
                    false, default(ImageBytes), Color.White);
            {
                var radius = 0.008f;
                var length = 0.2f;
                _manipulator = new Mesh(
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 0, true, new Color4(1, 0, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 0, false, new Color4(0.5f, 0, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 1, true, new Color4(0, 1, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 1, false, new Color4(0, 0.5f, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 2, true, new Color4(0, 0, 1.0f, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 2, false, new Color4(0, 0, 0.5f, 1)))
                    );
            }

            // cursor(drag position)
            {
                _cursor = new Mesh(
                    new Submesh(nodepth, D3D11MeshFactory.CreateCube(0.01f)));
            }

            // Matrixが変化したら再描画
            _selected.Subscribe(x =>
            {
                if (_subscription != null)
                {
                    _subscription.Dispose();
                    _subscription = null;
                }

                if (x != null)
                {
                    _subscription = x.LocalMatrixObservable.Subscribe(y =>
                    {
                        _updated.OnNext(Unit.Default);
                    });
                }
            });
        }
        IDisposable _subscription;

        protected override void OnUpdated(Source source)
        {
            LoadAsset(source);
        }

        async void LoadAsset(Source source)
        {
            if (source.GLTF == null)
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

        public void Draw(D3D11Device device, Matrix camera)
        {
            foreach (var node in _gizmos)
            {
                node.Draw(device, camera);
            }

            #region Scene
            foreach (var node in _drawables)
            {
                node.Update(Matrix.Identity);
            }

            if (_asset != null)
            {
                _asset.UpdateSkins();
            }

            foreach (var node in _drawables)
            {
                node.Draw(device, camera);
            }
            #endregion

            if (Selected != null)
            {
                var s = Matrix.Scaling(1.0f);
                _manipulator.Draw(device/*, camera, s * Selected.WorldMatrix*/);
                _cursor.Draw(device/*, camera, s * Matrix.Translation(_cursorPosition)*/);
            }
        }

        public void StartDrag(Camera camera, float x, float y)
        {
            var ray = camera.GetRay(x, y);

            if (Selected == null)
            {
                _index = -1;
                return;
            }
            Logger.Debug(ray);

            var i = default(SubmeshIntersection?);
            foreach (var t in _manipulator.Intersect(Selected.WorldMatrix, ray))
            {
                if (!i.HasValue || t.Triangle.Distance < i.Value.Triangle.Distance)
                {
                    i = t;
                }
            }
            if (!i.HasValue)
            {
                _index = -1;
                return;
            }

            // Start Drag
            _index = i.Value.SubmeshIndex;

            Logger.Debug($"Intersect {_index}");
        }

        public void EndDrag()
        {
            var node = Selected;
            if (node == null)
            {
                // arienai
                return;
            }

            var m = node.WorldMatrix;
            m.Row4 = new Vector4(_cursorPosition, 1);

            if (node.Parent != null)
            {
                var p = node.Parent.WorldMatrix;
                p.Invert();
                m = m * p;
            }

            node.LocalMatrix = m;
        }

        public bool Manipulate(Camera camera, float x, float y)
        {
            if (Selected == null)
            {
                return false;
            }
            if (_index == -1)
            {
                return false;
            }

            var center = camera.GetRay(0, 0).Direction;
            var plane = new Plane((Vector3)Selected.WorldMatrix.Row4,
                center
                );
            var ray = camera.GetRay(x, y);
            ray.Intersects(ref plane, out _cursorPosition);

            switch (_index)
            {
                case 0:
                case 1:
                    {
                        var w = Selected.WorldMatrix;
                        var o = (Vector3)w.Row4;
                        var axis = (Vector3)w.Row1;
                        _cursorPosition = o + axis * Vector3.Dot((_cursorPosition - o), axis);
                    }
                    break;

                case 2:
                case 3:
                    {
                        var w = Selected.WorldMatrix;
                        var o = (Vector3)w.Row4;
                        var axis = (Vector3)w.Row2;
                        _cursorPosition = o + axis * Vector3.Dot((_cursorPosition - o), axis);
                    }
                    break;

                case 4:
                case 5:
                    {
                        var w = Selected.WorldMatrix;
                        var o = (Vector3)w.Row4;
                        var axis = (Vector3)w.Row3;
                        _cursorPosition = o + axis * Vector3.Dot((_cursorPosition - o), axis);
                    }
                    break;

                default:
                    {
                    }
                    break;
            }

            return true;
        }
    }
}
