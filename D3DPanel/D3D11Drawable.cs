using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace D3DPanel
{
    struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TexCoord0;

        public static int Stride = Marshal.SizeOf<Vertex>();
    }

    public class D3D11Drawable : System.IDisposable
    {
        public PrimitiveTopology Topology => PrimitiveTopology.TriangleList;

        int[] m_indices;
        Vertex[] m_vertices;

        public D3D11Drawable(int[] indices, D3D11Shader shader, Vector3[] positions, Vector3[] normals, Vector2[] uvs)
        {
            m_shader = shader;

            m_indices = indices;
            m_vertices = new Vertex[positions.Length];
            for (int i = 0; i < m_vertices.Length; ++i)
            {
                m_vertices[i].Position = positions[i];
                if (normals != null) m_vertices[i].Normal = normals[i];
                if (uvs != null) m_vertices[i].TexCoord0 = new Vector4(uvs[i].X, uvs[i].Y, 0, 0);
            }
        }

        D3D11Shader m_shader;
        Buffer m_indexBuffer;
        Buffer m_vertexBuffer;

        public void Dispose()
        {
            if (m_indexBuffer != null)
            {
                m_indexBuffer.Dispose();
                m_indexBuffer = null;
            }
            if (m_vertexBuffer != null)
            {
                m_vertexBuffer.Dispose();
                m_vertexBuffer = null;
            }
        }

        public void Draw(D3D11Renderer renderer)
        {
            var device = renderer.Device;
            var context = renderer.Context;
            m_shader.SetupContext(device, context);

            context.InputAssembler.PrimitiveTopology = Topology;

            if (m_indexBuffer == null)
            {
                m_indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, m_indices);
            }
            context.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            if (m_vertexBuffer == null)
            {
                m_vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, m_vertices);
            }
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, Vertex.Stride, 0));

            //context.DrawIndexed(m_indices.Length, 0, 0);
            context.Draw(m_vertices.Length, 0);
        }
    }
}
