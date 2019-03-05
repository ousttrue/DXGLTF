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

        public void UpdateWorldConstants<T>(T value) where T : struct
        {
            m_context.VertexShader.SetConstantBuffer(0, m_worldConstants.Update(m_device, m_context,
                value));
        }

        struct ObjectConstants
        {
            public Color4 Color;
        }
        public void SetMaterial(D3D11Material material)
        {
            m_context.PixelShader.SetConstantBuffer(0, m_objectConstants.Update(m_device, m_context,
                new ObjectConstants
                {
                    Color = material.Color
                }
                ));
            //UpdateObjectConstants(material.Color);

            // shader
            material.Shader.SetupContext(Device, Context);

            // material
            Context.PixelShader.SetShaderResource(0, material.GetOrCreateSRV(Device));
            Context.PixelShader.SetSampler(0, material.GetOrCreateSamplerState(Device));
            Context.Rasterizer.State = material.GetRasterizerState(Device);
            Context.OutputMerger.SetDepthStencilState(material.GetOrCreateDepthStencilState(Device));
        }

        public bool SetVertices(D3D11Shader shader, D3D11Mesh mesh)
        {
            var inputs = shader.InputElements.Value;
            if (inputs == null)
            {
                return false;
            }
            if (!mesh.HasPositionAttribute)
            {
                return false;
            }

            Context.InputAssembler.PrimitiveTopology = mesh.Topology;

            var vertices = mesh.GetVertexBuffer(Device, Context, inputs);

            Context.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(vertices, mesh.Stride, 0));
            return true;
        }

        public bool SetIndices(D3D11Mesh mesh)
        {
            var indexBuffer = mesh.GetIndexBuffer(Device);
            if (indexBuffer == null)
            {
                return false;
            }
            Context.InputAssembler.SetIndexBuffer(indexBuffer,
                SharpDX.DXGI.Format.R32_UInt, 0);
            return true;
        }

        public void Draw(int offset, int count)
        {
            Context.Draw(count, offset);
        }

        public void DrawIndexed(int offset, int count)
        {
            Context.DrawIndexed(count, offset, 0);
        }

        #region Resource
        SharpDX.Direct3D11.Device m_device;
        public SharpDX.Direct3D11.Device Device => m_device;
        DXGISwapChain m_swapChain;

        DeviceContext m_context;
        public DeviceContext Context => m_context;

        class Constants : System.IDisposable
        {
            Buffer m_constantBuffer;

            public void Dispose()
            {
                if (m_constantBuffer != null)
                {
                    m_constantBuffer.Dispose();
                    m_constantBuffer = null;
                }
            }

            public Buffer Update<T>(SharpDX.Direct3D11.Device m_device, DeviceContext context,
                T value) where T : struct
            {
                if (m_constantBuffer == null)
                {
                    m_constantBuffer = Buffer.Create(m_device, BindFlags.ConstantBuffer, ref value);
                    m_constantBuffer.DebugName = typeof(T).Name;
                }
                context.UpdateSubresource(ref value, m_constantBuffer);
                return m_constantBuffer;
            }
        }
        Constants m_worldConstants = new Constants();
        Constants m_objectConstants = new Constants();

        public void Dispose()
        {
            if (m_swapChain != null)
            {
                m_swapChain.Dispose();
            }

            if (m_worldConstants != null)
            {
                m_worldConstants.Dispose();
                m_worldConstants = null;
            }

            if (m_objectConstants != null)
            {
                m_objectConstants.Dispose();
                m_objectConstants = null;
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

        RenderTargetView _rtv;
        DepthStencilView _dsv;

        public void ClearDepth()
        {
            m_context.ClearDepthStencilView(_dsv,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f, 0);
        }

        public void Begin(System.IntPtr hWnd, Color4 clear)
        {
            if (m_device == null)
            {
                CreateDevice(hWnd);
            }

            (_rtv, _dsv) = m_swapChain.GetRenderTarget(m_device);
            //var clear = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 128, 0);
            m_context.ClearRenderTargetView(_rtv, clear);
            ClearDepth();

            m_context.OutputMerger.SetTargets(_dsv, _rtv);
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
