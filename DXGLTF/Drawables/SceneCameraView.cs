using D3DPanel;
using DXGLTF.Assets;
using NLog;
using SharpDX;


namespace DXGLTF.Drawables
{
    public class SceneCameraView: IDrawable
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        Camera _camera = new Camera
        {
            View = Matrix.Identity,
        };

        #region Rect
        LocalRect _rect = new LocalRect();
        public void SetLocalRect(int x, int y, int w, int h)
        {
            DisposeSizeDependent();

            _camera.Resize(w, h);
            _rect.SetLocalRect(x, y, w, h);
        }

        public bool IsOnRect(int x, int y)
        {
            return _rect.IsOnRect(x, y);
        }

        public int Width => _rect.Width;
        public int Height => _rect.Height;
        #endregion

        Scene _scene;
        int _index = -1;
        Node _manipulator;
        Node _cursor;
        Vector3 _cursorPosition;

        public void DisposeSizeDependent()
        {
        }

        public void Dispose()
        {
            if (_manipulator != null)
            {
                _manipulator.Dispose();
                _manipulator = null;
            }

            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
            }

            /*
            if (_scene != null)
            {
                _scene.Dispose();
                _scene = null;
            }
            */
        }

        public SceneCameraView(Scene scene)
        {
            _scene = scene;

            // manipulator
            var nodepth = new D3D11Material("manipulator", ShaderLoader.Instance.CreateShader(ShaderType.Gizmo),
                    false, default(ImageBytes), Color.White);
            {
                var radius = 0.008f;
                var length = 0.2f;
                _manipulator = new Node(new Mesh(
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 0, true, new Color4(1, 0, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 0, false, new Color4(0.5f, 0, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 1, true, new Color4(0, 1, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 1, false, new Color4(0, 0.5f, 0, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 2, true, new Color4(0, 0, 1.0f, 1))),
                    new Submesh(nodepth, D3D11MeshFactory.CreateArrow(radius, length, 2, false, new Color4(0, 0, 0.5f, 1)))
                    ));
            }

            // cursor(drag position)
            {
                _cursor = new Node(new Mesh(
                    new Submesh(nodepth, D3D11MeshFactory.CreateCube(0.01f))));
            }
        }

        public void Update(D3D11Device device)
        {
            _camera.Update();
        }

        public void Draw(D3D11Device device, int left, int top)
        {
            device.SetViewport(new Viewport(left, top, _rect.Width, _rect.Height));

            var camera = _camera.View * _camera.Projection;

            _scene.Draw(device, camera);

            if (_scene.Selected != null)
            {
                var s = Matrix.Scaling(1.0f);
                _manipulator.Draw(device, _scene.Selected.WorldMatrix * camera);
                _cursor.Draw(device, Matrix.Translation(_cursorPosition) * camera);
            }
        }

        #region Mouse
        public bool MouseMove(int x, int y)
        {
            var invalidate = false;
            var deltaX = x - _rect.MouseX;
            var deltaY = y - _rect.MouseY;
            _rect.MouseMove(x, y);

            if (_rect.IsMouseLeftDown)
            {
                // drag
                if (Manipulate(x, y))
                {
                    invalidate = true;
                }
            }

            if (_rect.IsMouseRightDown)
            {
                _camera.YawPitch(deltaX, deltaY);
                invalidate = true;
            }

            if (_rect.IsMouseMiddleDown)
            {
                _camera.Shift(deltaX, deltaY);
                invalidate = true;
            }

            return invalidate;
        }

        public bool MouseLeftDown(int x, int y)
        {
            var invalidate = false;
            if (!_rect.IsMouseLeftDown)
            {
                StartDrag(_camera, x, y);
                invalidate = true;
            }
            _rect.MouseLeftDown(x, y);
            return invalidate;
        }

        public bool MouseLeftUp(int x, int y)
        {
            var invalidate = false;
            if (_rect.IsMouseLeftDown)
            {
                EndDrag();
                invalidate = true;
            }
            _rect.MouseLeftUp(x, y);
            return invalidate;
        }

        public bool MouseMiddleDown(int x, int y)
        {
            _rect.MouseMiddleDown(x, y);
            return false;
        }

        public bool MouseMiddleUp(int x, int y)
        {
            _rect.MouseMiddleUp(x, y);
            return false;
        }

        public bool MouseRightDown(int x, int y)
        {
            _rect.MouseRightDown(x, y);
            return false;
        }

        public bool MouseRightUp(int x, int y)
        {
            _rect.MouseRightUp(x, y);
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

            if (_scene.Selected == null)
            {
                _index = -1;
                return;
            }
            Logger.Debug(ray);

            var i = default(SubmeshIntersection?);
            foreach (var t in _manipulator.Mesh.Intersect(_scene.Selected.WorldMatrix, ray))
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
            var node = _scene.Selected;
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
            if (_scene.Selected == null)
            {
                return false;
            }
            if (_index == -1)
            {
                return false;
            }

            var center = _camera.GetRay(0, 0).Direction;
            var plane = new Plane((Vector3)_scene.Selected.WorldMatrix.Row4,
                center
                );
            var ray = _camera.GetRay(x, y);
            ray.Intersects(ref plane, out _cursorPosition);

            switch (_index)
            {
                case 0:
                case 1:
                    {
                        var w = _scene.Selected.WorldMatrix;
                        var o = (Vector3)w.Row4;
                        var axis = (Vector3)w.Row1;
                        _cursorPosition = o + axis * Vector3.Dot((_cursorPosition - o), axis);
                    }
                    break;

                case 2:
                case 3:
                    {
                        var w = _scene.Selected.WorldMatrix;
                        var o = (Vector3)w.Row4;
                        var axis = (Vector3)w.Row2;
                        _cursorPosition = o + axis * Vector3.Dot((_cursorPosition - o), axis);
                    }
                    break;

                case 4:
                case 5:
                    {
                        var w = _scene.Selected.WorldMatrix;
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
