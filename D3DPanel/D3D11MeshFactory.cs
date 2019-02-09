using SharpDX;
using System.Collections.Generic;

namespace D3DPanel
{
    public static class D3D11MeshFactory
    {
        public static D3D11Mesh CreateTriangle(float size = 0.5f)
        {
            var drawable = new D3D11Mesh(SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                new[] { 0, 1, 2 });
            drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(0.0f, size, 0),
                    new Vector3(size, -size, 0),
                    new Vector3(-size, -size, 0),
                }));
            drawable.SetAttribute(Semantics.COLOR, VertexAttribute.Create(new Color4[]{
                    new Color4(1.0f, 0, 0, 1.0f),
                    new Color4(0, 1.0f, 0, 1.0f),
                    new Color4(0, 0, 1.0f, 1.0f),
                }));
            return drawable;
        }

        public static D3D11Mesh CreateAxis(float w, float h)
        {
            var offset = 0.001f;
            var arrow = w * 2;
            var drawable = new D3D11Mesh(SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                new[] {
                    0, 1, 2,
                    2, 3, 0,
                    4, 5, 6,

                    7, 8, 9,
                    9, 10, 7,
                    11, 12, 13
                });

            {
                // X
                drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(w, offset, -w),
                    new Vector3(h, offset, -w),
                    new Vector3(h, offset, w),
                    new Vector3(w, offset, w),

                    new Vector3(h, offset, -arrow),
                    new Vector3(h+arrow, offset, 0),
                    new Vector3(h, offset, arrow),

                    new Vector3(-w, offset, w),
                    new Vector3(-w, offset, h),
                    new Vector3(w, offset, h),
                    new Vector3(w, offset, w),

                    new Vector3(-arrow, offset, h),
                    new Vector3(0, offset, h+arrow),
                    new Vector3(arrow, offset, h),
                }));
                var red = new Color4(1.0f, 0, 0, 1.0f);
                var blue = new Color4(0, 0, 1.0f, 1.0f);
                drawable.SetAttribute(Semantics.COLOR, VertexAttribute.Create(new Color4[]{
                    red,
                    red,
                    red,
                    red,
                    red,
                    red,
                    red,
                    blue,
                    blue,
                    blue,
                    blue,
                    blue,
                    blue,
                    blue,
                }));
            }
            return drawable;
        }

        public static D3D11Mesh CreateGrid(float size, int count)
        {
            var positions = new List<Vector3>();
            var colors = new List<Color4>();
            var indices = new List<int>();
            var length = size * count;

            var white = Color4.White;
            var red = new Color4(1, 0, 0, 1);
            var blue = new Color4(0, 0, 1, 1);

            // vertical
            for (int i = -count; i <= count; ++i)
            {
                indices.Add(positions.Count);
                positions.Add(new Vector3(i * size, 0, -length));

                indices.Add(positions.Count);
                positions.Add(new Vector3(i * size, 0, length));

                if (i == 0)
                {
                    colors.Add(blue);
                    colors.Add(blue);
                }
                else
                {
                    colors.Add(white);
                    colors.Add(white);
                }
            }
            // horizontal
            for (int i = -count; i <= count; ++i)
            {
                indices.Add(positions.Count);
                positions.Add(new Vector3(-length, 0, i * size));

                indices.Add(positions.Count);
                positions.Add(new Vector3(length, 0, i * size));

                if (i == 0)
                {
                    colors.Add(red);
                    colors.Add(red);
                }
                else
                {
                    colors.Add(white);
                    colors.Add(white);
                }
            }

            var drawable = new D3D11Mesh(SharpDX.Direct3D.PrimitiveTopology.LineList,
                indices.ToArray());
            drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(positions.ToArray()));
            drawable.SetAttribute(Semantics.COLOR, VertexAttribute.Create(colors.ToArray()));
            return drawable;
        }
    }
}
