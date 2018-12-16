using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;


namespace D3DPanel
{
    public class D3D11Drawable : System.IDisposable
    {
        public PrimitiveTopology Topology => PrimitiveTopology.TriangleList;

        public D3D11Drawable(D3D11Shader shader)
        {
            m_shader = shader;
        }

        D3D11Shader m_shader;
        Buffer m_vertices;

        public void Dispose()
        {
            if (m_shader != null)
            {
                m_shader.Dispose();
            }
            if (m_vertices != null)
            {
                m_vertices.Dispose();
                m_vertices = null;
            }
        }

        public void Draw(Device device, DeviceContext context)
        {
            m_shader.SetupContext(device, context);

            context.InputAssembler.PrimitiveTopology = Topology;

            if (m_vertices == null)
            {
                m_vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                                      {
                                      new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
                                  });
            }
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertices, 32, 0));

            context.Draw(3, 0);
        }
    }
}
