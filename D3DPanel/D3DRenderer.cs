using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;


namespace D3DPanel
{
    public class D3DRenderer
    {
        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        SharpDX.Direct3D11.Device m_device;
        DeviceContext m_context;

        #region SwapChain
        SwapChain m_swapChain;
        Texture2D m_backBuffer;
        RenderTargetView m_renderView;
        void ClearRenderTarget()
        {
            if (m_renderView != null)
            {
                m_renderView.Dispose();
            }
            if (m_backBuffer != null)
            {
                m_backBuffer.Dispose();
            }
        }

        void GetRenderTarget()
        {
            if (m_swapChain == null)
            {
                return;
            }
            // New RenderTargetView from the backbuffer
            m_backBuffer = Texture2D.FromSwapChain<Texture2D>(m_swapChain, 0);
            m_renderView = new RenderTargetView(m_device, m_backBuffer);
        }
        #endregion

        public void Resize(int width, int height)
        {
            if (width == Width && height == Height)
            {
                return;
            }
            Width = width;
            Height = height;

            if (m_swapChain == null)
            {
                return;
            }
            ClearRenderTarget();
            var desc = m_swapChain.Description;
            m_swapChain.ResizeBuffers(desc.BufferCount, Width, Height, desc.ModeDescription.Format, desc.Flags);
            GetRenderTarget();
        }

        public void Paint(IntPtr hWnd)
        {
            if (m_device == null)
            {
                CreateDevice(hWnd);
            }

            var clear = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 128, 0);
            m_context.ClearRenderTargetView(m_renderView, clear);
            m_swapChain.Present(0, PresentFlags.None);
        }

        void CreateDevice(IntPtr hWnd)
        {
            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                                   new ModeDescription(Width, Height,
                                                       new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = hWnd,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
                DeviceCreationFlags.Debug, 
                desc,
                out m_device, out m_swapChain);
            m_context = m_device.ImmediateContext;

            // Ignore all windows events
            var factory = m_swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(hWnd, WindowAssociationFlags.IgnoreAll);

            GetRenderTarget();
        }
    }
}
