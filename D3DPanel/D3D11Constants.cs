
using SharpDX.Direct3D11;


namespace D3DPanel
{
    public class D3D11Constants<T> : System.IDisposable
        where T : struct
    {
        Buffer _buffer;

        public void Dispose()
        {
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }
        }

        void Update(D3D11Device device, T value)
        {
            if (_buffer == null)
            {
                _buffer = Buffer.Create(device.Device, BindFlags.ConstantBuffer, ref value);
                _buffer.DebugName = typeof(T).Name;
            }
            device.Context.UpdateSubresource(ref value, _buffer);
        }

        public void SetVSConstants(D3D11Device device, int slot, T value)
        {
            Update(device, value);
            device.Context.VertexShader.SetConstantBuffer(slot, _buffer);
        }

        public void SetPSConstants(D3D11Device device, int slot, T value)
        {
            Update(device, value);
            device.Context.PixelShader.SetConstantBuffer(slot, _buffer);
        }
    }
}
