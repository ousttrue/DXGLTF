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

        public D3D11RenderTarget()
        { }

        //m_swapChain.Description.SampleDescription,
        public void CreateFromTexture(Texture2D texture, int count, int quality)
        {
            Dispose();

            var device = texture.Device;
            _rtv = new RenderTargetView(texture.Device, texture);
            {
                var desc = texture.Description;
                using (var depthBuffer = new Texture2D(device, new Texture2DDescription
                {
                    Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = desc.Width,
                    Height = desc.Height,
                    SampleDescription = new SharpDX.DXGI.SampleDescription
                    {
                        Count = count,
                        Quality = quality
                    },
                    BindFlags = BindFlags.DepthStencil
                }))
                {
                    var depthDesc = new DepthStencilViewDescription
                    {
                    };
                    if (count > 1 ||
                        quality > 0)
                    {
                        depthDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampled;
                    }
                    else
                    {
                        depthDesc.Dimension = DepthStencilViewDimension.Texture2D;
                    }
                    _dsv = new DepthStencilView(device, depthBuffer, depthDesc);
                }
            }
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
