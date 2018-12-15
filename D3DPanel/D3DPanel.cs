using System;
using System.Windows.Forms;


namespace D3DPanel
{
    public partial class D3DPanel: UserControl
    {
        D3DRenderer m_renderer = new D3DRenderer();

        public D3DPanel()
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
        }
    }
}
