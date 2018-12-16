using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace D3DPanel
{
    public class D3D11Renderer : IDisposable
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

        public D3D11Renderer()
        {
            string vsPath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MiniTri.fx");
            string psPath = vsPath;
            var vsSource = File.ReadAllText(vsPath, Encoding.UTF8);
            var psSource = default(string);
            if (vsPath == psPath)
            {
                psSource = vsSource;
            }
            else
            {
                psSource = File.ReadAllText(psPath, Encoding.UTF8);
            }

            m_drawables.Add(new D3D11Drawable(new D3D11Shader(vsSource, psSource)));
        }

        #region Resource
        SharpDX.Direct3D11.Device m_device;
        DeviceContext m_context;
        List<D3D11Drawable> m_drawables = new List<D3D11Drawable>();

        public void Dispose()
        {
            foreach (var d in m_drawables)
            {
                d.Dispose();
            }
            m_drawables.Clear();

            ClearRenderTarget();

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

            // Camera
            if (m_renderView == null)
            {
                GetRenderTarget();
            }
            m_context.OutputMerger.SetTargets(m_renderView);
            m_context.Rasterizer.SetViewport(Viewport);

            foreach (var d in m_drawables)
            {
                d.Draw(m_device, m_context);
            }

            m_swapChain.Present(0, PresentFlags.None);
        }

        void CreateDevice(IntPtr hWnd)
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

            GetRenderTarget();
        }
    }
}
