using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;


namespace D3DPanel
{
    public class DXGISwapChain : IDisposable
    {
        SwapChain m_swapChain;
        public DXGISwapChain(SwapChain swapChain)
        {
            m_swapChain = swapChain;
        }

        RenderTargetView m_renderView;
        DepthStencilView m_depthStencilView;

        public void Dispose()
        {
            ClearRenderTarget();

            if (m_swapChain != null)
            {
                m_swapChain.Dispose();
                m_swapChain = null;
            }
        }

        void ClearRenderTarget()
        {
            if (m_depthStencilView != null)
            {
                m_depthStencilView.Dispose();
                m_depthStencilView = null;
            }
            if (m_renderView != null)
            {
                m_renderView.Dispose();
                m_renderView = null;
            }
        }

        public void Present()
        {
            m_swapChain.Present(0, PresentFlags.None);
        }

        public void Resize(int width, int height)
        {
            ClearRenderTarget();
            if (m_swapChain == null)
            {
                return;
            }
            var desc = m_swapChain.Description;
            m_swapChain.ResizeBuffers(desc.BufferCount, width, height,
                desc.ModeDescription.Format, desc.Flags);
        }

        public (RenderTargetView, DepthStencilView) GetRenderTarget(SharpDX.Direct3D11.Device device)
        {
            if (m_swapChain == null)
            {
                return (null, null);
            }

            if (m_renderView == null)
            {
                // New RenderTargetView from the backbuffer
                using (var backBuffer = Texture2D.FromSwapChain<Texture2D>(m_swapChain, 0))
                {
                    backBuffer.DebugName = "backBuffer";
                    m_renderView = new RenderTargetView(device, backBuffer);

                    if (m_depthStencilView == null)
                    {
                        var desc = backBuffer.Description;
                        using (var depthBuffer = new Texture2D(device, new Texture2DDescription
                        {
                            Format = Format.D24_UNorm_S8_UInt,
                            ArraySize = 1,
                            MipLevels = 1,
                            Width = desc.Width,
                            Height = desc.Height,
                            SampleDescription = m_swapChain.Description.SampleDescription,
                            BindFlags = BindFlags.DepthStencil
                        }))
                        {
                            var depthDesc = new DepthStencilViewDescription
                            {
                            };
                            if (m_swapChain.Description.SampleDescription.Count > 1 ||
                                m_swapChain.Description.SampleDescription.Quality > 0)
                            {
                                depthDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampled;
                            }
                            else
                            {
                                depthDesc.Dimension = DepthStencilViewDimension.Texture2D;
                            }
                            m_depthStencilView = new DepthStencilView(device, depthBuffer, depthDesc);
                        }
                    }
                }
            }

            return (m_renderView, m_depthStencilView);
        }
    }
}
