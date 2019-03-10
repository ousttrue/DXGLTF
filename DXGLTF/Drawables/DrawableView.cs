using D3DPanel;


namespace DXGLTF.Drawables
{
    public class DrawableView : RenderTargetRect
    {
        public IDrawable _drawable;

        public override void Dispose()
        {
            if (_drawable != null)
            {
                _drawable.Dispose();
                _drawable = null;
            }

            base.Dispose();
        }

        public DrawableView(IDrawable drawable)
        {
            _drawable = drawable;
        }

        public override void Update(D3D11Device device)
        {
            SetRenderTarget(device);
            if (_drawable != null)
            {
                _drawable.Update(device);
                _drawable.Draw(device, 0, 0);
            }
        }

        public override void SetLocalRect(int x, int y, int w, int h)
        {
            base.Dispose();

            _drawable?.SetLocalRect(x, y, w, h);
        }

        public override bool IsOnRect(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            else
            {
                return _drawable.IsOnRect(x, y);
            }
        }

        public override int Width
        {
            get
            {
                if (_drawable == null)
                {
                    return 1;
                }
                else
                {
                    return _drawable.Width;
                }
            }
        }

        public override int Height
        {
            get
            {
                if (_drawable == null)
                {
                    return 1;
                }
                else
                {
                    return _drawable.Height;
                }
            }
        }

        public override bool MouseLeftDown(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseLeftDown(x, y);
        }

        public override bool MouseMiddleDown(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseMiddleDown(x, y);
        }

        public override bool MouseRightDown(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseRightDown(x, y);
        }

        public override bool MouseLeftUp(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseLeftUp(x, y);
        }

        public override bool MouseMiddleUp(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseMiddleUp(x, y);
        }

        public override bool MouseRightUp(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseRightUp(x, y);
        }

        public override bool MouseMove(int x, int y)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseMove(x, y);
        }

        public override bool MouseWheel(int d)
        {
            if (_drawable == null)
            {
                return false;
            }
            return _drawable.MouseWheel(d);
        }
    }
}
