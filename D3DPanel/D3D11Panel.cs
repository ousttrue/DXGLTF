using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace D3DPanel
{
    public partial class D3D11Panel : UserControl
    {
        D3D11Device _device;

        List<D3D11Mesh> m_drawables = new List<D3D11Mesh>();

        public void ClearDrawables()
        {
            foreach (var d in m_drawables)
            {
                d.Dispose();
            }
            m_drawables.Clear();
        }

        public D3D11Panel()
        {
            InitializeComponent();
        }

        private void D3DPanel_Paint(object sender, PaintEventArgs e)
        {
            /*
            m_renderer.Begin(Handle, new SharpDX.Color4(0.5f, 0.5f, 0.5f, 0));
            foreach(var d in m_drawables)
            {
                //d.Draw(m_renderer);
            }           
            m_renderer.End();
            */
        }

        private void D3DPanel_SizeChanged(object sender, EventArgs e)
        {
            //m_renderer.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }
    }
}
