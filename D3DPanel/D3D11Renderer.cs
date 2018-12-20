using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;


namespace D3DPanel
{
    public class D3D11Renderer : System.IDisposable
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

        Viewport Viewport
        {
            get
            {
                return new Viewport(0, 0, Width, Height, 0.0f, 1.0f);
            }
        }

        #region Resource
        SharpDX.Direct3D11.Device m_device;
        public SharpDX.Direct3D11.Device Device => m_device;
        DXGISwapChain m_swapChain;

        DeviceContext m_context;
        public DeviceContext Context => m_context;

        Buffer m_constantBuffer;

        public void Dispose()
        {
            if (m_swapChain != null)
            {
                m_swapChain.Dispose();
            }

            if (m_constantBuffer != null)
            {
                m_constantBuffer.Dispose();
                m_constantBuffer = null;
            }

            if (m_context != null)
            {
                m_context.Dispose();
                m_context = null;
            }
            if (m_device != null)
            {
                m_device.Dispose();
                m_device = null;
            }
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
            m_swapChain.Resize(Width, Height);
        }

        public void Begin(System.IntPtr hWnd, Camera camera)
        {
            if (m_device == null)
            {
                CreateDevice(hWnd);
            }

            // Camera
            var rtv = m_swapChain.GetRenderTarget(m_device);
            var clear = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 128, 0);
            m_context.ClearRenderTargetView(rtv, clear);

            if (m_constantBuffer == null)
            {
                m_constantBuffer = Buffer.Create(m_device, BindFlags.ConstantBuffer, ref camera.ViewProjection);
                m_constantBuffer.DebugName = "Constant";
            }
            m_context.UpdateSubresource(ref camera.ViewProjection, m_constantBuffer);
            m_context.VertexShader.SetConstantBuffer(0, m_constantBuffer);

            m_context.OutputMerger.SetTargets(rtv);
            m_context.Rasterizer.SetViewport(Viewport);
        }

        public void End()
        {
            m_swapChain.Present();
        }

        void CreateDevice(System.IntPtr hWnd)
        {
            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(Width, Height,
                    new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = hWnd,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SwapChain swapChain;
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
                DeviceCreationFlags.Debug,
                desc,
                out m_device, out swapChain);
            m_context = m_device.ImmediateContext;
            m_swapChain = new DXGISwapChain(swapChain);

            // Ignore all windows events
            using (var factory = swapChain.GetParent<Factory>())
            {
                factory.MakeWindowAssociation(hWnd, WindowAssociationFlags.IgnoreAll);
            }
        }
    }
}
