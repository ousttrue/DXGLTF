﻿using D3DPanel;
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

        D3D11Renderer _renderer = new D3D11Renderer();
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
            _renderer.Dispose();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do nothing
        }

        private void D3DContent_Paint(object sender, PaintEventArgs e)
        {
            _camera.Update();
            _renderer.Begin(Handle, new Color4(0.5f, 0.5f, 0.5f, 0));

            _hierarchy.Draw(_renderer, _camera);

            _renderer.End();
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            _renderer.Resize(ClientSize.Width, ClientSize.Height);
            _camera.Resize(ClientSize.Width, ClientSize.Height);
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
