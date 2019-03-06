using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using System.Linq;
using SharpDX;
using System.Runtime.InteropServices;


namespace D3DPanel
{
    public enum Semantics
    {
        POSITION,
        NORMAL,
        TEXCOORD,
        COLOR,
    }

    public class D3D11Mesh : IDisposable
    {
        public PrimitiveTopology Topology
        {
            get;
            private set;
        }

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

        #region IndexBuffer
        int[] m_indices;
        public int IndexCount
        {
            get { return m_indices != null ? m_indices.Length : 0; }
        }
        SharpDX.Direct3D11.Buffer m_indexBuffer;
        SharpDX.Direct3D11.Buffer GetIndexBuffer(Device device)
        {
            if (m_indices == null)
            {
                return null;
            }
            else
            {
                if (m_indexBuffer == null)
                {
                    m_indexBuffer = SharpDX.Direct3D11.Buffer.Create(device,
                        BindFlags.IndexBuffer, m_indices);
                    m_indexBuffer.DebugName = "IndexBuffer";
                }
                return m_indexBuffer;
            }
        }
        public bool SetIndices(D3D11Device device)
        {
            var indexBuffer = GetIndexBuffer(device.Device);
            if (indexBuffer == null)
            {
                return false;
            }
            device.Context.InputAssembler.SetIndexBuffer(indexBuffer,
                SharpDX.DXGI.Format.R32_UInt, 0);
            return true;
        }
        #endregion

        #region VertexBuffer
        Dictionary<Semantics, VertexAttribute> m_attributes = new Dictionary<Semantics, VertexAttribute>();
        public void SetAttribute(Semantics semantics, VertexAttribute attribute)
        {
            m_attributes[semantics] = attribute;

            if (semantics == Semantics.POSITION)
            {
                VertexCount = attribute.Value.Count / attribute.ElementSize;

                _positions = new Vector3[VertexCount];
                var pinnedArray = GCHandle.Alloc(_positions, GCHandleType.Pinned);
                {
                    var ptr = pinnedArray.AddrOfPinnedObject();
                    var positions = attribute.Value;
                    Marshal.Copy(positions.Array, positions.Offset, ptr, positions.Count);
                }
                pinnedArray.Free();
            }
        }
        Vector3[] _positions;

        public bool HasPositionAttribute
        {
            get
            {
                return m_attributes.ContainsKey(Semantics.POSITION);
            }
        }

        SharpDX.Direct3D11.Buffer m_vertexBuffer;

        public int Stride
        {
            get;
            private set;
        }

        public int VertexCount
        {
            get;
            private set;
        }

        InterleavedBuffer _buffer;
        InputElement[] _inputs;
        SharpDX.Direct3D11.Buffer GetVertexBuffer(D3D11Device device,
            InputElement[] inputs)
        {
            if (inputs != _inputs)
            {
                Dispose();
                _inputs = inputs;
            }

            if (m_vertexBuffer == null)
            {
                Stride = inputs.Sum(y => GetSize(y));
                var pos = m_attributes[Semantics.POSITION];

                // fill buffer
                _buffer = new InterleavedBuffer(Stride, VertexCount);
                int offset = 0;
                foreach (var input in inputs)
                {
                    VertexAttribute attr;
                    var semantics = (Semantics)Enum.Parse(typeof(Semantics), input.SemanticName, true);
                    if (m_attributes.TryGetValue(semantics, out attr))
                    {
                        _buffer.Set(attr.Value, attr.ElementSize, offset);
                    }
                    offset += GetSize(input);
                }

                var desc = new BufferDescription
                {
                    Usage = ResourceUsage.Dynamic,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    SizeInBytes = _buffer.Buffer.Length,
                };
                m_vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device.Device, _buffer.Buffer, desc);
                m_vertexBuffer.DebugName = "VertexBuffer";
            }

            if (_skinnedPosition != null)
            {
                //
                // skinning
                //
                var box = device.Context.MapSubresource(m_vertexBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);

                int offset = 0;
                foreach (var input in inputs)
                {
                    // search position sematntics
                    VertexAttribute attr;
                    var semantics = (Semantics)Enum.Parse(typeof(Semantics), input.SemanticName, true);
                    if (semantics == Semantics.POSITION)
                    {
                        if (m_attributes.TryGetValue(semantics, out attr))
                        {
                            _buffer.Set(_skinnedPosition, offset);
                        }
                        break;
                    }
                    offset += GetSize(input);
                }

                Marshal.Copy(_buffer.Buffer, 0, box.DataPointer, _buffer.Buffer.Length);

                device.Context.UnmapSubresource(m_vertexBuffer, 0);
            }

            return m_vertexBuffer;
        }
        public bool SetVertices(D3D11Device device, D3D11Shader shader)
        {
            var inputs = shader.InputElements.Value;
            if (inputs == null)
            {
                return false;
            }
            if (!HasPositionAttribute)
            {
                return false;
            }

            device.Context.InputAssembler.PrimitiveTopology = Topology;

            var vertices = GetVertexBuffer(device, inputs);

            device.Context.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(vertices, Stride, 0));
            return true;
        }
        #endregion

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

        #region Skinniing
        ushort[] _joints;
        public void SetJoints(ushort[] joints)
        {
            _joints = joints;
        }

        float[] _weights;
        public void SetWeights(float[] weights)
        {
            _weights = weights;
        }

        Vector3[] _skinnedPosition;
        public void Skinning(Matrix[] matrices)
        {
            if (matrices == null)
            {
                return;
            }
            if (!HasPositionAttribute)
            {
                return;
            }
            if (_skinnedPosition == null)
            {
                _skinnedPosition = new Vector3[_positions.Length];
            }

            for (int i = 0; i < _positions.Length; ++i)
            {
                var transformed = Vector3.Transform(_positions[i], matrices[_joints[i * 4]]);
                _skinnedPosition[i] = (Vector3)transformed;
            }
        }
        #endregion

        public D3D11Mesh(PrimitiveTopology topology, int[] indices)
        {
            m_indices = indices;
            Topology = topology;
        }

        public void DrawIndexed(D3D11Device device, int offset, int count)
        {
            device.Context.DrawIndexed(count, offset, 0);
        }

        public void Draw(D3D11Device device, int offset, int count)
        {
            device.Context.Draw(count, offset);
        }

        public IEnumerable<TriangleIntersection> Intersect(Ray ray)
        {
            if (Topology != PrimitiveTopology.TriangleList)
            {
                yield break;
            }

            if (m_indices == null)
            {
                for (int i = 0; i < VertexCount; i += 2)
                {
                    var i1 = i + 1;
                    var i2 = i + 2;
                    var d = default(float);
                    if (ray.Intersects(
                        ref _positions[i],
                        ref _positions[i1],
                        ref _positions[i2],
                        out d))
                    {
                        yield return new TriangleIntersection
                        {
                            I0 = i,
                            I1 = i1,
                            I2 = i2,
                            Distance = d
                        };
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_indices.Length; i += 3)
                {
                    var i0 = m_indices[i];
                    var i1 = m_indices[i + 1];
                    var i2 = m_indices[i + 2];
                    var d = default(float);
                    if (ray.Intersects(
                        ref _positions[i0],
                        ref _positions[i1],
                        ref _positions[i2],
                        out d))
                    {
                        yield return new TriangleIntersection
                        {
                            I0 = i0,
                            I1 = i1,
                            I2 = i2,
                            Distance = d
                        };
                    }
                }
            }
        }
    }
}
