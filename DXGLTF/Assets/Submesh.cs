using D3DPanel;
using System;


namespace DXGLTF.Assets
{
    public struct Submesh : IDisposable
    {
        public D3D11Material Material;
        public D3D11Mesh Mesh;
        public int DrawIndexOffset;
        public int DrawIndexCount;

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

        public Submesh(D3D11Material material, D3D11Mesh mesh)
            : this(material, mesh, 0, 0)
        {
        }

        public Submesh(D3D11Material material, D3D11Mesh mesh,
            int offset, int count)
        {
            Material = material;
            Mesh = mesh;
            DrawIndexOffset = offset;
            DrawIndexCount = count;
        }

        public void DrawSubmesh(D3D11Device device)
        {
            // material constants
            Material.Setup(device);

            Mesh.DrawIndexed(device, DrawIndexOffset, DrawIndexCount);
        }

        public void Draw(D3D11Device device)
        {
            // material constants
            Material.Setup(device);

            if (Mesh.SetIndices(device))
            {
                Mesh.DrawIndexed(device, 0, Mesh.IndexCount);
            }
            else
            {
                Mesh.Draw(device, 0, Mesh.VertexCount);
            }
        }
    }
}
