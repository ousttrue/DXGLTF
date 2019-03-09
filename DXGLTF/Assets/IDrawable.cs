using D3DPanel;
using System;


namespace DXGLTF.Assets
{
    public interface IDrawable : IDisposable
    {
        void SetLocalRect(int x, int y, int w, int h);
        bool MouseLeftDown(int x, int y);
        bool MouseMiddleDown(int x, int y);
        bool MouseRightDown(int x, int y);
        bool MouseLeftUp(int x, int y);
        bool MouseMiddleUp(int x, int y);
        bool MouseRightUp(int x, int y);
        bool MouseMove(int x, int y);
        bool MouseWheel(int d);

        void Draw(D3D11Device device);
    }
}
