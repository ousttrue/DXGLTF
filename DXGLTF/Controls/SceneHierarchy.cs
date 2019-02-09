using DXGLTF.Assets;
using DXGLTF.Nodes;
using GltfScene;
using NLog;
using System;
using System.Collections.Generic;
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
                var tmp = default(AssetContext);
                lock (this)
                {
                    tmp = _asset;
                    _asset = value;
                }

                if (tmp != null)
                {
                    tmp.Dispose();
                }
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
            /*
            var unlit = _shaderLoader.CreateShader(ShaderType.Unlit);
            var gizmo = _shaderLoader.CreateShader(ShaderType.Gizmo);

            // default triangle
            _drawables.Add(new Node(D3D11DrawableFactory.CreateTriangle(gizmo)));

            // gizmos
            _gizmos.Add(new Node(D3D11DrawableFactory.CreateAxis(gizmo, 0.1f, 10.0f)));
            _gizmos.Add(new Node(D3D11DrawableFactory.CreateGrid(gizmo, 1.0f, 10)));
            */
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

        protected override void OnSelected(TreeNode node)
        {
        }
    }
}
