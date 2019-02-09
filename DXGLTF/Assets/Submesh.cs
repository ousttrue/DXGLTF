using D3DPanel;
using SharpDX;
using System;


namespace DXGLTF.Assets
{
    public struct Submesh : IDisposable
    {
        public D3D11Material Material;
        public D3D11Mesh Mesh;

        public void Dispose()
        {
            if (Material != null)
            {
                Material.Dispose();
                Material = null;
            }
            if (Mesh != null)
            {
                Mesh.Dispose();
                Mesh = null;
            }
        }

        public Submesh(D3D11Shader shader, D3D11Mesh mesh)
        {
            Material = new D3D11Material(shader, default(ImageBytes), Color4.White);
            Mesh = mesh;
        }
    }
}
