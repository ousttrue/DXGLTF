using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace D3DPanel
{
    public class InterleavedBuffer
    {
        byte[] m_buffer;
        public byte[] Buffer
        {
            get { return m_buffer; }
        }

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

        public InterleavedBuffer(int stride, int count)
        {
            m_buffer = new byte[stride * count];
            Stride = stride;
            VertexCount = m_buffer.Length / stride;
        }

        public void Set(ArraySegment<byte> values, int stride, int offset)
        {
            for (int pos = 0; pos < values.Count; pos += stride, offset += Stride)
            {
                System.Buffer.BlockCopy(values.Array, values.Offset + pos, m_buffer, offset, stride);
            }
        }

        public void Set<T>(T[] values, int offset)where T: struct
        {
            var size = Marshal.SizeOf(typeof(T));
            using (var pin = UniGLTF.Pin.Create(values))
            {
                var src = 0;
                for(int i=0; i<values.Length; ++i)
                {
                    Marshal.Copy(IntPtr.Add(pin.Ptr, src), m_buffer, offset, size);
                    src += size;
                    offset += Stride;
                }
            }
        }

        public void SetPosition(ArraySegment<byte> values, int stride, int offset, Matrix[] matrices)
        {
            if (matrices == null)
            {
                Set(values, stride, offset);
            }
            else
            {
                for (int pos = 0; pos < values.Count; pos += stride, offset += Stride)
                {
                    System.Buffer.BlockCopy(values.Array, values.Offset + pos, m_buffer, offset, stride);
                }
            }
        }
    }

    public struct VertexAttribute
    {
        public ArraySegment<byte> Value;
        public int ElementSize;

        public VertexAttribute(ArraySegment<byte> value, int elementSize)
        {
            Value = value;
            ElementSize = elementSize;
        }

        public static VertexAttribute Create<T>(T[] values) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var va = new VertexAttribute
            {
                Value = new ArraySegment<byte>(new byte[size * values.Length]),
                ElementSize = size,
            };
            using (var pin = UniGLTF.Pin.Create(values))
            {
                Marshal.Copy(pin.Ptr, va.Value.Array, va.Value.Offset, va.Value.Count);
            }
            return va;
        }
    }
}
