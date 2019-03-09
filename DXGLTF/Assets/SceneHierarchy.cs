using D3DPanel;
using NLog;
using Reactive.Bindings;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace DXGLTF.Assets
{
    public interface IDrawable: IDisposable
    {
        void SetScreenSize(int w, int h);
        void Draw(D3D11Device device);
    }

    public interface IMouseObserver
    {
        bool MouseLeftDown(int x, int y);
        bool MouseMiddleDown(int x, int y);
        bool MouseRightDown(int x, int y);
        bool MouseLeftUp(int x, int y);
        bool MouseMiddleUp(int x, int y);
        bool MouseRightUp(int x, int y);
        bool MouseMove(int x, int y);
        bool MouseWheel(int d);
    }

    public class SceneHierarchy : IDrawable, IMouseObserver
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
                    _drawables.AddRange(_asset.Roots);
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

        public void Dispose()
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

        IDisposable _subscription;
        public SceneHierarchy()
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

        Camera _camera = new Camera
        {
            View = Matrix.Identity,
        };

        public void SetScreenSize(int w, int h)
        {
            _camera.Resize(w, h);
        }

        public void Draw(D3D11Device device)
        {
            _camera.Update();

            var camera = _camera.View * _camera.Projection;

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

        #region Mouse
        int m_mouseX;
        int m_mouseY;
        bool m_leftDown;
        bool m_middleDown;
        bool m_rightDown;
        public bool MouseMove(int x, int y)
        {
            var invalidate = false;
            if (m_mouseX != 0 && m_mouseY != 0)
            {
                var deltaX = x - m_mouseX;
                var deltaY = y - m_mouseY;

                if (m_leftDown)
                {
                    // drag
                    if (Manipulate(x, y))
                    {
                        invalidate = true;
                    }
                }

                if (m_rightDown)
                {
                    _camera.YawPitch(deltaX, deltaY);
                    invalidate = true;
                }

                if (m_middleDown)
                {
                    _camera.Shift(deltaX, deltaY);
                    invalidate = true;
                }
            }
            m_mouseX = x;
            m_mouseY = y;
            return invalidate;
        }

        public bool MouseLeftDown(int x, int y)
        {
            var invalidate = false;
            if (!m_leftDown)
            {
                StartDrag(_camera, x, y);
                invalidate = true;
            }
            m_leftDown = true;
            return invalidate;
        }

        public bool MouseLeftUp(int x, int y)
        {
            var invalidate = false;
            if (m_leftDown)
            {
                EndDrag();
                invalidate = true;
            }
            m_leftDown = false;
            return invalidate;
        }

        public bool MouseMiddleDown(int x, int y)
        {
            m_middleDown = true;
            return false;
        }

        public bool MouseMiddleUp(int x, int y)
        {
            m_middleDown = false;
            return false;
        }

        public bool MouseRightDown(int x, int y)
        {
            m_rightDown = true;
            return false;
        }

        public bool MouseRightUp(int x, int y)
        {
            m_rightDown = false;
            return false;
        }

        public bool MouseWheel(int d)
        {
            _camera.Dolly(d);
            return true;
        }

        void StartDrag(Camera camera, float x, float y)
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

        void EndDrag()
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

        bool Manipulate(float x, float y)
        {
            if (Selected == null)
            {
                return false;
            }
            if (_index == -1)
            {
                return false;
            }

            var center = _camera.GetRay(0, 0).Direction;
            var plane = new Plane((Vector3)Selected.WorldMatrix.Row4,
                center
                );
            var ray = _camera.GetRay(x, y);
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
        #endregion
    }
}
