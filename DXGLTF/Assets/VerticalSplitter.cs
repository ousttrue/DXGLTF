using D3DPanel;


namespace DXGLTF.Assets
{
    public class VerticalSplitter : IDrawable
    {
        IDrawable _top;
        IDrawable _bottom;
        IDrawable _target;

        public VerticalSplitter(IDrawable top, IDrawable bottom)
        {
            _top = top;
            _bottom = bottom;

            _target = top;
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

        public void Draw(D3D11Device device, int left, int top)
        {
            if (_top != null)
            {
                // SetRenderTarget
                _top.Draw(device, left, top);
            }

            if (_bottom != null)
            {
                // SetRenderTarget
                _bottom.Draw(device, left, top + _rect.Height / 2);
            }
        }

        LocalRect _rect=new LocalRect();
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
        public bool MouseLeftDown(int x, int y)
        {
            _rect.MouseLeftDown(x, y);
            if (_target == null)
            {
                if (_top != null && _top.IsOnRect(x, y))
                {
                    _target = _top;
                }
                else if(_bottom!=null && _bottom.IsOnRect(x, y))
                {
                    _target = _bottom;
                }
                return false;
            }

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
            _rect.MouseMiddleDown(x, y);
            if (_target == null)
            {
                if (_top != null && _top.IsOnRect(x, y))
                {
                    _target = _top;
                }
                else if (_bottom != null && _bottom.IsOnRect(x, y))
                {
                    _target = _bottom;
                }
                return false;
            }

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
            _rect.MouseRightDown(x, y);
            if (_target == null)
            {
                if (_top != null && _top.IsOnRect(x, y))
                {
                    _target = _top;
                }
                else if (_bottom != null && _bottom.IsOnRect(x, y))
                {
                    _target = _bottom;
                }
                return false;
            }

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
