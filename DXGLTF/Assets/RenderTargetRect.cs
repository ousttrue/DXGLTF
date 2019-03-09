using D3DPanel;
using SharpDX;


namespace DXGLTF.Assets
{
    public class RenderTargetRect : IDrawable
    {
        D3D11RenderTarget _renderTarget;
        Mesh _node;
        public readonly IDrawable Drawable;
        public void Dispose()
        {
            if (_node != null)
            {
                _node.Dispose();
                _node = null;
            }
            if (_renderTarget != null)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
            }
        }

        public RenderTargetRect(IDrawable drawable)
        {
            Drawable = drawable;
        }

        public Color4 ClearColor
        {
            get;
            set;
        }

        public void Update(D3D11Device device)
        {
            if (Drawable == null)
            {
                return;
            }

            if (_renderTarget == null)
            {
                _renderTarget = new D3D11RenderTarget();
                _renderTarget.Create(device, Drawable.Width, Drawable.Height);

                var shader = ShaderLoader.Instance.CreateShader(ShaderType.Screen);
                var material = new D3D11Material("rect", shader);
                var mesh = D3D11MeshFactory.CreateQuadrangle();
                _node = new Mesh(new Submesh(material, mesh));
                _node.Submeshes[0].Material.CreateSRV(_renderTarget);
            }
            _renderTarget.Setup(device, ClearColor);

            if (Drawable != null)
            {
                Drawable.Update(device);
                Drawable.Draw(device, 0, 0);
            }
        }

        public void Draw(D3D11Device device, int left, int top)
        {
            if (Drawable == null)
            {
                return;
            }

            device.SetViewport(new Viewport(left, top, Width, Height));

            var x = (float)left / Drawable.Width;
            var y = (float)top / Drawable.Height;
            _node.Draw(device);
        }

        public void SetLocalRect(int x, int y, int w, int h)
        {
            Dispose();

            Drawable?.SetLocalRect(x, y, w, h);
        }

        public bool IsOnRect(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            else
            {
                return Drawable.IsOnRect(x, y);
            }
        }

        public int Width
        {
            get
            {
                if (Drawable == null)
                {
                    return 1;
                }
                else
                {
                    return Drawable.Width;
                }
            }
        }

        public int Height
        {
            get
            {
                if (Drawable == null)
                {
                    return 1;
                }
                else
                {
                    return Drawable.Height;
                }
            }
        }

        public bool MouseLeftDown(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseLeftDown(x, y);
        }

        public bool MouseMiddleDown(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseMiddleDown(x, y);
        }

        public bool MouseRightDown(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseRightDown(x, y);
        }

        public bool MouseLeftUp(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseLeftUp(x, y);
        }

        public bool MouseMiddleUp(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseMiddleUp(x, y);
        }

        public bool MouseRightUp(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseRightUp(x, y);
        }

        public bool MouseMove(int x, int y)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseMove(x, y);
        }

        public bool MouseWheel(int d)
        {
            if (Drawable == null)
            {
                return false;
            }
            return Drawable.MouseWheel(d);
        }
    }
}
