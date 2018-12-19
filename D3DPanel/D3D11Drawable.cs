using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using System.Linq;


namespace D3DPanel
{
    public enum Semantics
    {
        POSITION,
    }

    public class D3D11Drawable : System.IDisposable
    {
        public PrimitiveTopology Topology => PrimitiveTopology.TriangleList;

        int[] m_indices;
        Dictionary<Semantics, VertexAttribute> m_attributes = new Dictionary<Semantics, VertexAttribute>();

        static int GetSize(InputElement e)
        {
            switch (e.Format)
            {
                case SharpDX.DXGI.Format.R32G32B32A32_Float:
                    return 16;

                case SharpDX.DXGI.Format.R32G32B32_Float:
                    return 12;

                case SharpDX.DXGI.Format.R32G32_Float:
                    return 8;
            }

            throw new NotImplementedException();
        }


        public D3D11Drawable(int[] indices, D3D11Shader shader)
        {
            m_shader = shader;
            m_indices = indices;

            m_shader.InputElements.Subscribe(x =>
            {
                Dispose();
            });
        }

        public void SetAttribute(Semantics semantics, VertexAttribute attribute)
        {
            m_attributes[semantics] = attribute;
        }

        D3D11Shader m_shader;
        SharpDX.Direct3D11.Buffer m_indexBuffer;
        SharpDX.Direct3D11.Buffer m_vertexBuffer;

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

        int m_stride;
        int m_vertexCount;
        public void Draw(D3D11Renderer renderer)
        {
            var device = renderer.Device;
            var context = renderer.Context;
            m_shader.SetupContext(device, context);

            var inputs = m_shader.InputElements.Value;
            if (inputs == null)
            {
                return;
            }

            if (!m_attributes.ContainsKey(Semantics.POSITION))
            {
                return;
            }

            context.InputAssembler.PrimitiveTopology = Topology;

            if (m_vertexBuffer == null)
            {
                m_stride = inputs.Sum(y => GetSize(y));
                var pos = m_attributes[Semantics.POSITION];
                m_vertexCount = pos.Value.Count / pos.ElementSize;
                var buffer = new InterleavedBuffer(m_stride, m_vertexCount);

                int offset = 0;
                foreach(var input in inputs)
                {
                    buffer.Set(pos.Value, pos.ElementSize, offset);
                    offset += GetSize(input);
                }
                m_vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, buffer.Buffer);
            }
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, m_stride, 0));

            if (m_indices == null)
            {
                context.Draw(m_vertexCount, 0);
            }
            else
            {
                if (m_indexBuffer == null)
                {
                    m_indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, m_indices);
                }
                context.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
                context.DrawIndexed(m_indices.Length, 0, 0);
            }
        }
    }
}
