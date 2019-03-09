using SharpDX.DXGI;
using System;


namespace D3DPanel
{
    public class D3D11Device : IDisposable
    {
        IntPtr _hWnd;

        public SharpDX.Direct3D11.Device Device
        {
            get;
            private set;
        }

        public SharpDX.Direct3D11.DeviceContext Context
        {
            get;
            private set;
        }

        public DXGISwapChain SwapChain
        {
            get;
            private set;
        }

        public void Dispose()
        {
            _hWnd = IntPtr.Zero;

            if (SwapChain != null)
            {
                SwapChain.Dispose();
                SwapChain = null;
            }
            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
            if (Device != null)
            {
                Device.Dispose();
                Device = null;
            }
        }

        public void SetHWND(IntPtr hWnd, int w, int h)
        {
            if (_hWnd == hWnd)
            {
                return;
            }
            Dispose();
            _hWnd = hWnd;

            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(w, h,
                    new Rational(60, 1),
                    Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = hWnd,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SharpDX.Direct3D11.Device device;
            SwapChain swapChain;
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                SharpDX.Direct3D11.DeviceCreationFlags.Debug,
                desc,
                out device, out swapChain);

            // Ignore all windows events
            using (var factory = swapChain.GetParent<Factory>())
            {
                factory.MakeWindowAssociation(hWnd, WindowAssociationFlags.IgnoreAll);
            }

            Device = device;
            Context = Device.ImmediateContext;
            SwapChain = new DXGISwapChain(swapChain);
        }

        public void Present()
        {
            Context.OutputMerger.SetTargets(
                (SharpDX.Direct3D11.DepthStencilView)null, 
                (SharpDX.Direct3D11.RenderTargetView)null);
            SwapChain.Present();
        }

        public void SetViewport(SharpDX.Viewport viewport)
        {
            Context.Rasterizer.SetViewport(viewport);
        }
    }
}
