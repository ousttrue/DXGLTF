using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;


namespace D3DPanel
{
    public class Camera
    {
        const float ToRadians = (float)(System.Math.PI / 180);

        int m_screenWidth;
        int m_screenHeight;
        public void Resize(int w, int h)
        {
            m_screenWidth = w;
            m_screenHeight = h;
            AspectRatio = (float)w / h;
        }

        public Matrix View;
        public float Yaw;
        public float Pitch;
        public void YawPitch(int dx, int dy)
        {
            Yaw += ((float)dx / m_screenWidth);
            Pitch += ((float)dy / m_screenHeight);
        }

        const float SHIFT = 1.0f;
        public float ShiftX;
        public float ShiftY;
        public void Shift(int dx, int dy)
        {
            ShiftX += ((float)dx / m_screenWidth) * Distance * SHIFT;
            ShiftY += ((float)dy / m_screenWidth) * Distance * SHIFT;
        }
        public float Distance=5;
        public void Dolly(int delta)
        {
            if (delta < 0)
            {
                Distance *= 1.1f;
            }
            else if(delta>0)
            {
                Distance *= 0.9f;
            }
        }

        public Matrix Projection;
        public float FovY = 30.0f * ToRadians;
        public float AspectRatio = 1.0f;
        public float ZNear = 0.1f;
        public float ZFar = 10.0f;

        public Matrix ViewProjection;

        public void Update()
        {
            View = Matrix.Translation(ShiftX, -ShiftY, -Distance) * Matrix.RotationYawPitchRoll(Yaw, Pitch, 0);
            Projection = Matrix.PerspectiveFovRH(FovY, AspectRatio, ZNear, ZFar);
            ViewProjection = View * Projection;
            ViewProjection.Transpose();
        }
    }

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

        Camera m_camera;

        #region Resource
        SharpDX.Direct3D11.Device m_device;
        public SharpDX.Direct3D11.Device Device => m_device;

        DeviceContext m_context;
        public DeviceContext Context => m_context;

        SharpDX.Direct3D11.Buffer m_constantBuffer;

        public void Dispose()
        {
            ClearRenderTarget();

            if (m_constantBuffer == null)
            {
                m_constantBuffer.Dispose();
                m_constantBuffer = null;
            }

            if (m_swapChain != null)
            {
                m_swapChain.Dispose();
                m_swapChain = null;
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

        #region SwapChain
        SwapChain m_swapChain;
        Texture2D m_backBuffer;
        RenderTargetView m_renderView;
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
        }

        public void Begin(System.IntPtr hWnd, Camera camera)
        {
            if (m_device == null)
            {
                CreateDevice(hWnd);
            }

            // Camera
            if (m_renderView == null)
            {
                GetRenderTarget();
            }
            var clear = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 128, 0);
            m_context.ClearRenderTargetView(m_renderView, clear);

            if (m_constantBuffer == null)
            {
                m_constantBuffer = Buffer.Create(m_device, BindFlags.ConstantBuffer, ref camera.ViewProjection);
            }
            m_context.UpdateSubresource(ref camera.ViewProjection, m_constantBuffer);
            m_context.VertexShader.SetConstantBuffer(0, m_constantBuffer);

            m_context.OutputMerger.SetTargets(m_renderView);
            m_context.Rasterizer.SetViewport(Viewport);
        }

        public void End()
        {
            //m_context.Flush();
            m_swapChain.Present(0, PresentFlags.None);
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
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
                DeviceCreationFlags.Debug,
                desc,
                out m_device, out m_swapChain);
            m_context = m_device.ImmediateContext;

            // Ignore all windows events
            var factory = m_swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(hWnd, WindowAssociationFlags.IgnoreAll);
        }
    }
}
