using System;
using System.Windows.Forms;


namespace D3DPanel
{
    public partial class D3D11Panel: UserControl
    {
        D3D11Renderer m_renderer = new D3D11Renderer();

        public D3D11Panel()
        {
            InitializeComponent();
        }

        private void D3DPanel_Paint(object sender, PaintEventArgs e)
        {
            m_renderer.Paint(Handle);
        }

        private void D3DPanel_SizeChanged(object sender, EventArgs e)
        {
            m_renderer.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }
    }
}
