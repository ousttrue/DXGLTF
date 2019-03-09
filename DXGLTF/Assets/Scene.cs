using D3DPanel;
using NLog;
using Reactive.Bindings;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace DXGLTF.Assets
{
    public class Scene
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        ReactiveProperty<Node> _selected = new ReactiveProperty<Node>();
        public Node Selected
        {
            get { return _selected.Value; }
            set { _selected.Value = value; }
        }
        public IObservable<Node> SelectedObservable
        {
            get { return _selected.AsObservable(); }
        }

        AssetContext _asset;
        public AssetContext Asset
        {
            set
            {
                if (_asset == value) return;

                // clear
                if (_asset != null)
                {
                    _asset.Dispose();
                }
                ClearDrawables();
                _selected.Value = null;

                // update
                _asset = value;
                if (_asset != null)
                {
                    var roots = _asset.Roots.ToArray();
                    _drawables.AddRange(roots);
                }

                _updated.OnNext(Unit.Default);
            }
        }

        Subject<Unit> _updated = new Subject<Unit>();
        public IObservable<Unit> Updated
        {
            get { return _updated.AsObservable(); }
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

        public void Dispose()
        {
            ClearDrawables();

            foreach (var x in _gizmos)
            {
                x.Dispose();
            }
            _gizmos.Clear();
        }

        IDisposable _subscription;
        public Scene()
        {
            _selected.Subscribe(_ =>
            {
                _updated.OnNext(Unit.Default);
            });

            var unlit = ShaderLoader.Instance.CreateShader(ShaderType.Unlit);
            var gizmo = ShaderLoader.Instance.CreateShader(ShaderType.Gizmo);

            // default triangle
            _drawables.Add(new Node(gizmo, D3D11MeshFactory.CreateTriangle()));

            // gizmos
            _gizmos.Add(new Node(gizmo, D3D11MeshFactory.CreateAxis(0.1f, 10.0f)));
            _gizmos.Add(new Node(gizmo, D3D11MeshFactory.CreateGrid(1.0f, 10)));

            // manipulator
            var nodepth = new D3D11Material("manipulator", ShaderLoader.Instance.CreateShader(ShaderType.Gizmo),
                    false, default(ImageBytes), Color.White);

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
        }
    }
}
