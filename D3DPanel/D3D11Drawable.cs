using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using System.Linq;
using SharpDX;

namespace D3DPanel
{
    public enum Semantics
    {
        POSITION,
        NORMAL,
        TEXCOORD,
        COLOR,
    }

    public class D3D11Drawable : IDisposable
    {
        #region Mesh
        public PrimitiveTopology Topology
        {
            get;
            private set;
        }
            
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
        #endregion

        #region Material
        // texture0
        ImageBytes m_textureBytes;

        // material color
        public Color4 Color
        {
            get;
            private set;
        }
        #endregion

        public D3D11Drawable(PrimitiveTopology topology, int[] indices, D3D11Shader shader, ImageBytes texture, Color4 color)
        {
            m_shader = shader;
            m_indices = indices;

            m_shader.InputElements.Subscribe(x =>
            {
                Dispose();
            });

            m_textureBytes = texture;
            Color = color;

            Topology = topology;
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
            if (m_shader != null)
            {
                m_shader.Dispose();
            }

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
            m_shader.SetupContext(device, context, m_textureBytes);

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
                    VertexAttribute attr;
                    var semantics = (Semantics)Enum.Parse(typeof(Semantics), input.SemanticName, true);
                    if (m_attributes.TryGetValue(semantics, out attr))
                    {
                        buffer.Set(attr.Value, attr.ElementSize, offset);
                    }
                    offset += GetSize(input);
                }
                m_vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, buffer.Buffer);
                m_vertexBuffer.DebugName = "VertexBuffer";
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
                    m_indexBuffer.DebugName = "IndexBuffer";
                }
                context.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
                context.DrawIndexed(m_indices.Length, 0, 0);
            }
        }
    }
}
