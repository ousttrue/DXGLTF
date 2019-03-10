using D3DPanel;


namespace DXGLTF.Drawables
{
    class D2DView : IDrawable
    {
        LocalRect _rect = new LocalRect();

        public int Width => _rect.Width;

        public int Height => _rect.Height;

        public void Dispose()
        {
        }

        public void Update(D3D11Device device)
        {
        }

        public void Draw(D3D11Device device, int left, int top)
        {
        }

        public bool IsOnRect(int x, int y)
        {
            return _rect.IsOnRect(x, y);
        }

        public bool MouseLeftDown(int x, int y)
        {
            _rect.MouseLeftDown(x, y);
            return false;
        }

        public bool MouseLeftUp(int x, int y)
        {
            _rect.MouseLeftUp(x, y);
            return false;
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

        public bool MouseMove(int x, int y)
        {
            _rect.MouseMove(x, y);
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
            return false;
        }

        public void SetLocalRect(int x, int y, int w, int h)
        {
            _rect.SetLocalRect(x, y, w, h);
        }
    }
}
