using D3DPanel;
using System;


namespace DXGLTF.Assets
{
    public interface IDrawable : IDisposable
    {
        void SetLocalRect(int x, int y, int w, int h);
        bool IsOnRect(int x, int y);
        bool MouseLeftDown(int x, int y);
        bool MouseMiddleDown(int x, int y);
        bool MouseRightDown(int x, int y);
        bool MouseLeftUp(int x, int y);
        bool MouseMiddleUp(int x, int y);
        bool MouseRightUp(int x, int y);
        bool MouseMove(int x, int y);
        bool MouseWheel(int d);
        void Draw(D3D11Device device, int left, int top);
    }

    /// <summary>
    /// スクリーン座標を管理する
    /// </summary>
    public class LocalRect
    {
        int _mx;
        int _my;
        bool _leftDown;
        bool _middleDown;
        bool _rightDown;
        public void MouseLeftDown(int x, int y)
        {
            _mx = x;
            _my = y;
            _leftDown = true;
        }

        public void MouseMiddleDown(int x, int y)
        {
            _mx = x;
            _my = y;
            _middleDown = true;
        }

        public void MouseRightDown(int x, int y)
        {
            _mx = x;
            _my = y;
            _rightDown = true;
        }

        public void MouseLeftUp(int x, int y)
        {
            _mx = x;
            _my = y;
            _leftDown = false;
        }

        public void MouseMiddleUp(int x, int y)
        {
            _mx = x;
            _my = y;
            _middleDown = false;
        }

        public void MouseRightUp(int x, int y)
        {
            _mx = x;
            _my = y;
            _rightDown = false;
        }

        public int _x;
        public int _y;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public void SetLocalRect(int x, int y, int w, int h)
        {
            _x = x;
            _y = y;
            Width = w;
            Height = h;
        }

        public bool IsOnRect(int x, int y)
        {
            if (x < _x) return false;
            if (x > _x + Width) return false;
            if (y < _y) return false;
            if (y > _y + Height) return false;
            return true;
        }
    }
}
