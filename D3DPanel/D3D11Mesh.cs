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
        public SharpDX.Direct3D11.Buffer GetIndexBuffer(Device device)
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

        InputElement[] _inputs;
        public SharpDX.Direct3D11.Buffer GetVertexBuffer(Device device,
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
                var buffer = new InterleavedBuffer(Stride, VertexCount);
                int offset = 0;
                foreach (var input in inputs)
                {
                    VertexAttribute attr;
                    var semantics = (Semantics)Enum.Parse(typeof(Semantics), input.SemanticName, true);
                    if (m_attributes.TryGetValue(semantics, out attr))
                    {
                        buffer.Set(attr.Value, attr.ElementSize, offset);
                    }
                    offset += GetSize(input);
                }

                m_vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, buffer.Buffer,
                    new BufferDescription
                    {
                        BindFlags = BindFlags.VertexBuffer
                    });
                m_vertexBuffer.DebugName = "VertexBuffer";
            }

            return m_vertexBuffer;
        }
        #endregion

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
        #endregion

        public D3D11Mesh(PrimitiveTopology topology, int[] indices)
        {
            m_indices = indices;
            Topology = topology;
        }

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
                _skinnedPosition[i] = _positions[i];
            }
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
