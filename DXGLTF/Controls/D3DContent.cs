using D3DPanel;
using DXGLTF.Assets;
using NLog;
using SharpDX;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class D3DContent : DockContent
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        D3D11Device _device = new D3D11Device();
        D3D11RenderTarget _backbuffer;

        IDrawable _drawable;

        public D3DContent(IDrawable drawable)
        {
            InitializeComponent();

            _drawable = drawable;
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
            _device.SetHWND(Handle, ClientSize.Width, ClientSize.Height);

            _drawable.Update(_device);

            if (_backbuffer == null)
            {
                _backbuffer = _device.SwapChain.CreateRenderTarget(_device);
            }
            _backbuffer.Setup(_device,
                new Color4(0.5f, 0.5f, 0.5f, 0)
                );
            _drawable.Draw(_device, 0, 0);

            _device.Present();
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            _drawable.SetLocalRect(0, 0, ClientSize.Width, ClientSize.Height);

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
            if(_drawable.MouseMove(e.X, e.Y))
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
                    if(_drawable.MouseLeftDown(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    Capture = true;
                    break;

                case MouseButtons.Middle:
                    m_middleDown = true;
                    if(_drawable.MouseMiddleDown(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    Capture = true;
                    break;

                case MouseButtons.Right:
                    m_rightDown = true;
                    if(_drawable.MouseRightDown(e.X, e.Y))
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
                    if(_drawable.MouseLeftUp(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    break;

                case MouseButtons.Middle:
                    m_middleDown = false;
                    if(_drawable.MouseMiddleUp(e.X, e.Y))
                    {
                        Invalidate();
                    }
                    break;

                case MouseButtons.Right:
                    m_rightDown = false;
                    if(_drawable.MouseRightUp(e.X, e.Y))
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
            if (_drawable.MouseWheel(e.Delta))
            {
                Invalidate();
            }
        }
        #endregion
    }
}
