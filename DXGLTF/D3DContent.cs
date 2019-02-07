using D3DPanel;
using GltfScene;
using NLog;
using SharpDX;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using UniJSON;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class D3DContent : DockContent
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        D3D11Renderer m_renderer = new D3D11Renderer();
        Camera m_camera = new Camera
        {
            View = Matrix.Identity,
        };

        JsonD3DConverter m_jsonD3D = new JsonD3DConverter();

        public D3DContent()
        {
            InitializeComponent();

            m_jsonD3D.UpdatedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ =>
                {
                    Invalidate();
                })
                ;
        }

        public void Shutdown()
        {
            m_jsonD3D.Dispose();
            m_renderer.Dispose();
        }

        public void SetSelection(Source source, ListTreeNode<JsonValue> node)
        {
            m_jsonD3D.SetSelection(source, node);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do nothing
        }

        private void D3DContent_Paint(object sender, PaintEventArgs e)
        {
            m_camera.Update();
            m_renderer.Begin(Handle);
            foreach(var node in m_jsonD3D.Drawables)
            {
                RendererDraw(node, Matrix.Identity);
            }
            m_renderer.End();
        }

        void RendererDraw(nodes.Node node, Matrix accumulated)
        {
            var m = node.LocalMatrix * accumulated;
            //Logger.Debug(m);
            foreach (var x in node.Value)
            {
                m_renderer.Draw(m_camera, x, m);
            }

            foreach (var child in node.Children)
            {
                RendererDraw(child, m);
            }
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            m_renderer.Resize(ClientSize.Width, ClientSize.Height);
            m_camera.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }

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
                    m_camera.YawPitch(deltaX, deltaY);
                    Invalidate();
                }

                if (m_middleDown)
                {
                    m_camera.Shift(deltaX, deltaY);
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
            m_camera.Dolly(e.Delta);
            Invalidate();
        }
    }
}
