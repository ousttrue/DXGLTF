using SharpDX;
using SharpDX.Direct3D11;
using System;


namespace D3DPanel
{
    public class D3D11Material: IDisposable
    {
        D3D11Shader m_shader;

        // texture0
        ImageBytes m_textureBytes;

        // material color
        public Color4 Color
        {
            get;
            private set;
        }

        public D3D11Material(D3D11Shader shader, ImageBytes texture, Color4 color)
        {
            m_shader = shader;
            m_shader.InputElements.Subscribe(x =>
            {
                Dispose();
            });

            m_textureBytes = texture;
            Color = color;
        }

        public void Dispose()
        {
            if (m_shader != null)
            {
                m_shader.Dispose();
                m_shader = null;
            }
        }

        public void Draw(D3D11Renderer renderer, D3D11Mesh mesh)
        {
            var device = renderer.Device;
            var context = renderer.Context;
            m_shader.SetupContext(device, context, m_textureBytes);

            var inputs = m_shader.InputElements.Value;
            if (inputs == null)
            {
                return;
            }

            if (!mesh.HasPositionAttribute)
            {
                return;
            }

            context.InputAssembler.PrimitiveTopology = mesh.Topology;


            context.InputAssembler.SetVertexBuffers(0, 
                new VertexBufferBinding(mesh.GetVertexBuffer(device, inputs), mesh.Stride, 0));

            var indexBuffer = mesh.GetIndexBuffer(device);
            if (indexBuffer == null)
            {
                context.Draw(mesh.VertexCount, 0);
            }
            else
            {
                context.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
                context.DrawIndexed(mesh.IndexCount, 0, 0);
            }
        }
    }
}
