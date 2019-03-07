using SharpDX;
using SharpDX.Direct3D11;
using System;


namespace D3DPanel
{
    public class D3D11RenderTarget : IDisposable
    {
        RenderTargetView _rtv;
        DepthStencilView _dsv;
        public void Dispose()
        {
            if (_rtv != null)
            {
                _rtv.Dispose();
                _rtv = null;
            }

            if (_dsv != null)
            {
                _dsv.Dispose();
                _dsv = null;
            }
        }

        public D3D11RenderTarget(RenderTargetView rtv, DepthStencilView dsv)
        {
            _rtv = rtv;
            _dsv = dsv;
        }

        public void Setup(D3D11Device device, Viewport viewport, Color4 clear)
        {
            //var clear = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 128, 0);
            device.Context.ClearRenderTargetView(_rtv, clear);
            device.Context.ClearDepthStencilView(_dsv,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f, 0);

            device.Context.OutputMerger.SetTargets(_dsv, _rtv);

            device.Context.Rasterizer.SetViewport(viewport);
        }
    }
}
