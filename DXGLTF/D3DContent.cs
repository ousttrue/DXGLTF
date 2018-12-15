using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using D3DPanel;

namespace DXGLTFContent
{
    public partial class D3DContent: DockContent
    {
        D3DRenderer m_renderer = new D3DRenderer();

        public D3DContent()
        {
            InitializeComponent();
        }

        private void D3DContent_Paint(object sender, PaintEventArgs e)
        {
            m_renderer.Paint(Handle);
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            m_renderer.Resize(ClientSize.Width, ClientSize.Height);
        }
    }
}
