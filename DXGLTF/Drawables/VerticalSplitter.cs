using D3DPanel;
using SharpDX;


namespace DXGLTF.Drawables
{
    public class VerticalSplitter : IDrawable
    {
        RenderTargetRect _top;
        RenderTargetRect _bottom;

        IDrawable _target;
        public VerticalSplitter(IDrawable top, IDrawable bottom)
        {
            _top = new RenderTargetRect(top)
            {
                ClearColor = new Color4(1, 0.5f, 0.5f, 1)
            }
            ;
            _bottom = new RenderTargetRect(bottom)
            {
                ClearColor = new Color4(0.5f, 1, 0.5f, 1)
            }
            ;
        }

        public void Dispose()
        {
            if (_top != null)
            {
                _top.Dispose();
                _top = null;
            }

            if (_bottom != null)
            {
                _bottom.Dispose();
                _bottom = null;
            }
        }

        public void Update(D3D11Device device)
        {
            if (_top != null)
            {
                _top.Update(device);
            }

            if (_bottom != null)
            {
                _bottom.Update(device);
            }
        }

        public void Draw(D3D11Device device, int left, int top)
        {
            if (_top != null)
            {
                _top.Draw(device, left, top);
            }

            if (_bottom != null)
            {
                _bottom.Draw(device, left, top + _rect.Height / 2);
            }
        }

        LocalRect _rect=new LocalRect();

        public int Width => _rect.Width;

        public int Height => _rect.Height;

        public void SetLocalRect(int x, int y, int w, int h)
        {
            _rect.SetLocalRect(x, y, w, h);

            h = h / 2;
            if (_top != null)
            {
                _top.SetLocalRect(x, y, w, h);
            }
            if (_bottom != null)
            {
                _bottom.SetLocalRect(x, y + h, w, h);
            }
        }

        public bool IsOnRect(int x, int y)
        {
            return _rect.IsOnRect(x, y);
        }

        #region Mouse
        void UpdateCaptureFocus(int x, int y)
        {
            if (_top == null && _bottom == null)
            {
                return;
            }
            else if (_bottom == null)
            {
                //Console.WriteLine(string.Format("UpdateFocus: Top"));
                _target = _top;
            }
            else if (_top == null)
            {
                //Console.WriteLine(string.Format("UpdateFocus: Bottom"));
                _target = _bottom;
            }
            else
            {
                if (_top.IsOnRect(x, y))
                {
                    //Console.WriteLine(string.Format("UpdateFocus: Top"));
                    _target = _top;
                }
                else if (_bottom.IsOnRect(x, y))
                {
                    //Console.WriteLine(string.Format("UpdateFocus: Bottom"));
                    _target = _bottom;
                }
            }
        }

        public bool MouseLeftDown(int x, int y)
        {
            if (!_rect.DownAny)
            {
                UpdateCaptureFocus(x, y);
            }
            _rect.MouseLeftDown(x, y);

            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseLeftDown(x, y);
            }
        }

        public bool MouseLeftUp(int x, int y)
        {
            _rect.MouseLeftUp(x, y);

            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseLeftUp(x, y);
            }
        }

        public bool MouseMiddleDown(int x, int y)
        {
            if (!_rect.DownAny)
            {
                UpdateCaptureFocus(x, y);
            }
            _rect.MouseMiddleDown(x, y);

            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseMiddleDown(x, y);
            }
        }

        public bool MouseMiddleUp(int x, int y)
        {
            _rect.MouseMiddleUp(x, y);

            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseMiddleUp(x, y);
            }
        }

        public bool MouseMove(int x, int y)
        {
            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseMove(x, y);
            }
        }

        public bool MouseRightDown(int x, int y)
        {
            if (!_rect.DownAny)
            {
                UpdateCaptureFocus(x, y);
            }
            _rect.MouseRightDown(x, y);

            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseRightDown(x, y);
            }
        }

        public bool MouseRightUp(int x, int y)
        {
            _rect.MouseRightUp(x, y);

            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseRightUp(x, y);
            }
        }

        public bool MouseWheel(int d)
        {
            if (_target == null)
            {
                return false;
            }
            else
            {
                return _target.MouseWheel(d);
            }
        }
        #endregion
    }
}
