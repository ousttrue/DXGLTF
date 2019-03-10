using D3DPanel;
using System;


namespace DXGLTF.Drawables
{
    public interface IDrawable : IDisposable
    {
        void SetLocalRect(int x, int y, int w, int h);
        bool IsOnRect(int x, int y);
        int Width { get; }
        int Height { get; }

        bool MouseLeftDown(int x, int y);
        bool MouseMiddleDown(int x, int y);
        bool MouseRightDown(int x, int y);
        bool MouseLeftUp(int x, int y);
        bool MouseMiddleUp(int x, int y);
        bool MouseRightUp(int x, int y);
        bool MouseMove(int x, int y);
        bool MouseWheel(int d);

        void Update(D3D11Device device);
        void Draw(D3D11Device device, int left, int top);
    }

    /// <summary>
    /// スクリーン座標を管理する
    /// </summary>
    public class LocalRect
    {
        #region Rect
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
        #endregion

        #region Mouse
        public int MouseX { get; private set; }
        public int MouseY { get; private set; }
        public bool IsMouseLeftDown { get; private set; }
        public bool IsMouseMiddleDown { get; private set; }
        public bool IsMouseRightDown { get; private set; }
        public bool DownAny
        {
            get { return IsMouseLeftDown || IsMouseMiddleDown || IsMouseRightDown; }
        }

        public void MouseLeftDown(int x, int y)
        {
            MouseX = x - _x;
            MouseY = y - _y;
            IsMouseLeftDown = true;
        }

        public void MouseMiddleDown(int x, int y)
        {
            MouseX = x - _x;
            MouseY = y - _y;
            IsMouseMiddleDown = true;
        }

        public void MouseRightDown(int x, int y)
        {
            MouseX = x - _x;
            MouseY = y - _y;
            IsMouseRightDown = true;
        }

        public void MouseLeftUp(int x, int y)
        {
            MouseX = x - _x;
            MouseY = y - _y;
            IsMouseLeftDown = false;
        }

        public void MouseMiddleUp(int x, int y)
        {
            MouseX = x - _x;
            MouseY = y - _y;
            IsMouseMiddleDown = false;
        }

        public void MouseRightUp(int x, int y)
        {
            MouseX = x - _x;
            MouseY = y - _y;
            IsMouseRightDown = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>x==y==0 なら dragしない</returns>
        public void MouseMove(int x, int y)
        {
            MouseX = x - _x;
            MouseY = y - _y;
        }
        #endregion
    }
}
