using D3DPanel;
using SharpDX.Direct2D1;


namespace DXGLTF.Drawables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/Direct2D/devices-and-device-contexts
    /// </summary>
    class D2DView : RenderTargetRect
    {
        LocalRect _rect = new LocalRect();
        Bitmap1 _bitmap;

        ID2DDrawable _scene = new D2DWriter();

        public override int Width => _rect.Width;

        public override int Height => _rect.Height;

        public override void Dispose()
        {
            _scene.Dispose();

            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }

            base.Dispose();
        }

        public override void Update(D3D11Device device)
        {
            var rt = SetRenderTarget(device);

            if (_bitmap == null)
            {
                var pf = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
                var bp = new BitmapProperties1(pf, device.Dpi.Height, device.Dpi.Width,
                    BitmapOptions.CannotDraw | BitmapOptions.Target)
                    ;

                using (var surface = rt.Surface)
                {
                    _bitmap = new Bitmap1(device.D2DDeviceContext, surface, bp);
                }
            }

            device.D2DDeviceContext.Target = _bitmap;
            device.D2DDeviceContext.BeginDraw();
            {
                _scene.Draw(device.D2DDeviceContext, new SharpDX.RectangleF(0, 0, _rect.Width, _rect.Height));
            }
            device.D2DDeviceContext.EndDraw();
            device.D2DDeviceContext.Target = null;
        }

        public override bool IsOnRect(int x, int y)
        {
            return _rect.IsOnRect(x, y);
        }

        public override bool MouseLeftDown(int x, int y)
        {
            _rect.MouseLeftDown(x, y);
            return false;
        }

        public override bool MouseLeftUp(int x, int y)
        {
            _rect.MouseLeftUp(x, y);
            return false;
        }

        public override bool MouseMiddleDown(int x, int y)
        {
            _rect.MouseMiddleDown(x, y);
            return false;
        }

        public override bool MouseMiddleUp(int x, int y)
        {
            _rect.MouseMiddleUp(x, y);
            return false;
        }

        public override bool MouseMove(int x, int y)
        {
            _rect.MouseMove(x, y);
            return true;
        }

        public override bool MouseRightDown(int x, int y)
        {
            _rect.MouseRightDown(x, y);
            return false;
        }

        public override bool MouseRightUp(int x, int y)
        {
            _rect.MouseRightUp(x, y);
            return false;
        }

        public override bool MouseWheel(int d)
        {
            return false;
        }

        public override void SetLocalRect(int x, int y, int w, int h)
        {
            Dispose();

            _rect.SetLocalRect(x, y, w, h);
        }
    }
}
