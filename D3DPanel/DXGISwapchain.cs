using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DPanel
{
    class DXGISwapChain: IDisposable
    {
        SwapChain m_swapChain;
        public DXGISwapChain(SwapChain swapChain)
        {
            m_swapChain = swapChain;
        }

        Texture2D m_backBuffer;
        RenderTargetView m_renderView;

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
            if (m_renderView != null)
            {
                m_renderView.Dispose();
                m_renderView = null;
            }
            if (m_backBuffer != null)
            {
                m_backBuffer.Dispose();
                m_backBuffer = null;
            }
        }

        public void Present()
        {
            m_swapChain.Present(0, PresentFlags.None);
        }

        public void Resize(int width, int height)
        {
            ClearRenderTarget();

            var desc = m_swapChain.Description;
            m_swapChain.ResizeBuffers(desc.BufferCount, width, height, 
                desc.ModeDescription.Format, desc.Flags);
        }

        public RenderTargetView GetRenderTarget(SharpDX.Direct3D11.Device device)
        {
            if (m_swapChain == null)
            {
                return null;
            }

            if (m_renderView == null)
            {
                // New RenderTargetView from the backbuffer
                m_backBuffer = Texture2D.FromSwapChain<Texture2D>(m_swapChain, 0);
                m_renderView = new RenderTargetView(device, m_backBuffer);
            }

            return m_renderView;
        }
    }
}
