using D3DPanel;
using DXGLTF.Assets;
using NLog;
using SharpDX;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class D3DContent : DockContent
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        D3D11Device _device = new D3D11Device();
        D3D11RenderTarget _backbuffer;
        D3D11RenderTarget _sceneRT;

        Scene _scene;

        public D3DContent(Scene scene)
        {
            InitializeComponent();

            _scene = scene;

            _scene.Updated
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ =>
                {
                    Invalidate();
                })
                ;
        }

        public void Shutdown()
        {
            if (_sceneRT != null)
            {
                _sceneRT.Dispose();
                _sceneRT = null;
            }

            if (_backbuffer != null)
            {
                _backbuffer.Dispose();
                _backbuffer = null;
            }

            _device.Dispose();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do nothing
        }

        private void D3DContent_Paint(object sender, PaintEventArgs e)
        {
            _device.SetHWND(Handle, ClientSize.Width, ClientSize.Height);

            if (_sceneRT == null)
            {
                _sceneRT = new D3D11RenderTarget();
                _sceneRT.Create(_device, ClientSize.Width, ClientSize.Height / 2);
            }

            if (_backbuffer == null)
            {
                _backbuffer = _device.SwapChain.CreateRenderTarget(_device);
            }
            _backbuffer.Setup(_device,
                new Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f),
                new Color4(0.5f, 0.5f, 0.5f, 0)
                );

            _scene.Draw(_device);

            _device.Present();
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            var w = ClientSize.Width;
            var h = ClientSize.Height / 2;

            _scene.SetScreenSize(w, h);

            if (_sceneRT != null)
            {
                _sceneRT.Dispose();
                _sceneRT = null;
            }

            if (_backbuffer != null)
            {
                _backbuffer.Dispose();
                _backbuffer = null;
            }

            if (_device.SwapChain != null)
            {
                _device.SwapChain.Resize(ClientSize.Width, ClientSize.Height);
            }

            Invalidate();
        }

        #region MouseEvents
        private void D3DContent_MouseMove(object sender, MouseEventArgs e)
        {
            if(_scene.MouseMove(e.X, e.Y))
            {
                Invalidate();
            }
        }

        bool m_leftDown;
        bool m_middleDown;
        bool m_rightDown;
        private void D3DContent_MouseDown(object sender, MouseEventArgs e)
        {
            Focus();

            switch (e.Button)
            {
                case MouseButtons.Left:
                    m_leftDown = true;
                    if(_scene.MouseLeftDown(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    Capture = true;
                    break;

                case MouseButtons.Middle:
                    m_middleDown = true;
                    if(_scene.MouseMiddleDown(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    Capture = true;
                    break;

                case MouseButtons.Right:
                    m_rightDown = true;
                    if(_scene.MouseRightDown(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    Capture = true;
                    break;
            }
        }

        private void D3DContent_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    m_leftDown = false;
                    if(_scene.MouseLeftUp(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    break;

                case MouseButtons.Middle:
                    m_middleDown = false;
                    if(_scene.MouseMiddleUp(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    break;

                case MouseButtons.Right:
                    m_rightDown = false;
                    if(_scene.MouseRightUp(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    break;
            }

            if (!m_leftDown
                && !m_middleDown
                && !m_rightDown)
            {
                Capture = false;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_scene.MouseWheel(e.Delta))
            {
                Invalidate();
            }
        }
        #endregion
    }
}
