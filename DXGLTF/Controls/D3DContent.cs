using D3DPanel;
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
        D3D11RenderTarget _sceneRT = new D3D11RenderTarget();

        Camera _camera = new Camera
        {
            View = Matrix.Identity,
        };

        SceneHierarchy _hierarchy;

        public D3DContent(SceneHierarchy hierarchy)
        {
            InitializeComponent();

            _hierarchy = hierarchy;

            _hierarchy.Updated
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ =>
                {
                    Invalidate();
                })
                ;
        }

        public void Shutdown()
        {
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
            _camera.Update();

            _device.SetHWND(Handle, ClientSize.Width, ClientSize.Height);

            if (_backbuffer == null)
            {
                _backbuffer = _device.SwapChain.CreateRenderTarget(_device);
            }
            _backbuffer.Setup(_device,
                new Viewport(0, 0, Width, Height, 0.0f, 1.0f),
                new Color4(0.5f, 0.5f, 0.5f, 0)
                );

            _hierarchy.Draw(_device, _camera);

            _device.Present();
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            _camera.Resize(ClientSize.Width, ClientSize.Height);

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
        int m_mouseX;
        int m_mouseY;
        private void D3DContent_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_mouseX != 0 && m_mouseY != 0)
            {
                var deltaX = e.X - m_mouseX;
                var deltaY = e.Y - m_mouseY;

                if (m_leftDown)
                {
                    // drag
                    if (_hierarchy.Manipulate(_camera, e.X, e.Y))
                    {
                        Invalidate();
                    }
                }

                if (m_rightDown)
                {
                    _camera.YawPitch(deltaX, deltaY);
                    Invalidate();
                }

                if (m_middleDown)
                {
                    _camera.Shift(deltaX, deltaY);
                    Invalidate();
                }
            }
            m_mouseX = e.X;
            m_mouseY = e.Y;
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
                    if (!m_leftDown)
                    {
                        _hierarchy.StartDrag(_camera, e.X, e.Y);
                        Invalidate();
                    }
                    m_leftDown = true;
                    Capture = true;
                    break;

                case MouseButtons.Middle:
                    m_middleDown = true;
                    Capture = true;
                    break;

                case MouseButtons.Right:
                    m_rightDown = true;
                    Capture = true;
                    break;
            }
        }

        private void D3DContent_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (m_leftDown)
                    {
                        _hierarchy.EndDrag();
                        Invalidate();
                    }
                    m_leftDown = false;
                    break;

                case MouseButtons.Middle:
                    m_middleDown = false;
                    break;

                case MouseButtons.Right:
                    m_rightDown = false;
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
            _camera.Dolly(e.Delta);
            Invalidate();
        }
        #endregion
    }
}
