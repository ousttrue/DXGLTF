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

        public void Dispose()
        {
            if (m_swapChain != null)
            {
                m_swapChain.Dispose();
                m_swapChain = null;
            }
        }

        public void Present()
        {
            m_swapChain.Present(0, PresentFlags.None);
        }

        public void Resize(int width, int height)
        {
            if (m_swapChain == null)
            {
                return;
            }
            var desc = m_swapChain.Description;
            m_swapChain.ResizeBuffers(desc.BufferCount, width, height,
                desc.ModeDescription.Format, desc.Flags);
        }

        public D3D11RenderTarget CreateRenderTarget(D3D11Device device)
        {
            // New RenderTargetView from the backbuffer
            var backBuffer = Texture2D.FromSwapChain<Texture2D>(m_swapChain, 0);
            backBuffer.DebugName = "backBuffer";
            var rt = new D3D11RenderTarget();
            rt.CreateFromTexture(backBuffer, 
                m_swapChain.Description.SampleDescription.Count,
                m_swapChain.Description.SampleDescription.Quality);
            return rt;
        }
    }
}
