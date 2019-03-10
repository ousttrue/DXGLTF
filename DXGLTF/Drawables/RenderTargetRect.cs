using D3DPanel;
using DXGLTF.Assets;
using SharpDX;


namespace DXGLTF.Drawables
{
    public abstract class RenderTargetRect : IDrawable
    {
        D3D11RenderTarget _renderTarget;
        Mesh _mesh;
        public virtual void Dispose()
        {
            if (_mesh != null)
            {
                _mesh.Dispose();
                _mesh = null;
            }
            if (_renderTarget != null)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
            }
        }

        public Color4 ClearColor
        {
            get;
            set;
        }

        public abstract void Update(D3D11Device device);

        protected D3D11RenderTarget SetRenderTarget(D3D11Device device)
        {
            if (_renderTarget == null)
            {
                _renderTarget = new D3D11RenderTarget();
                _renderTarget.Create(device, Width, Height);
            }
            _renderTarget.Setup(device, ClearColor);
            return _renderTarget;
        }

        public void Draw(D3D11Device device, int left, int top)
        {
            if (_renderTarget != null)
            {
                device.SetViewport(new Viewport(left, top, Width, Height));
                if (_mesh == null)
                {
                    var shader = ShaderLoader.Instance.CreateShader(ShaderType.Screen);
                    var material = new D3D11Material("rect", shader);
                    var mesh = D3D11MeshFactory.CreateQuadrangle();
                    _mesh = new Mesh(new Submesh(material, mesh));
                    _mesh.Submeshes[0].Material.CreateSRV(_renderTarget);
                }
                _mesh.Draw(device);
            }
        }

        public virtual void SetLocalRect(int x, int y, int w, int h)
        {
            Dispose();
        }

        public abstract bool IsOnRect(int x, int y);
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract bool MouseLeftDown(int x, int y);
        public abstract bool MouseMiddleDown(int x, int y);
        public abstract bool MouseRightDown(int x, int y);
        public abstract bool MouseLeftUp(int x, int y);
        public abstract bool MouseMiddleUp(int x, int y);
        public abstract bool MouseRightUp(int x, int y);
        public abstract bool MouseMove(int x, int y);
        public abstract bool MouseWheel(int d);
    }
}
