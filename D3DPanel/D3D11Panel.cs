using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace D3DPanel
{
    public partial class D3D11Panel: UserControl
    {
        D3D11Renderer m_renderer = new D3D11Renderer();

        List<D3D11Drawable> m_drawables = new List<D3D11Drawable>();
        public void AddDrawable(string vs, string ps)
        {
            var shader = new D3D11Shader(vs, ps);

            /*
            var positions = 
            new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
            m_drawables.Add(new D3D11Drawable(shader, ));
            */
        }

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
            m_renderer.Begin(Handle);
            foreach(var d in m_drawables)
            {
                d.Draw(m_renderer);
            }           
            m_renderer.End();
        }

        private void D3DPanel_SizeChanged(object sender, EventArgs e)
        {
            m_renderer.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }
    }
}
