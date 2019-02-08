using SharpDX;


namespace D3DPanel
{
    public static class D3D11DrawableFactory
    {
        public static D3D11Drawable CreateTriangle(D3D11Shader shader, float size = 0.5f)
        {
            var drawable = new D3D11Drawable(new[] { 0, 1, 2 },
                shader,
                default(ImageBytes),
                Color4.White
                );
            drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(0.0f, size, 0),
                    new Vector3(size, -size, 0),
                    new Vector3(-size, -size, 0),
                }));
            return drawable;
        }

        public static D3D11Drawable CreateAxis(D3D11Shader shader, float w, float h)
        {
            var arrow = w * 2;
            var drawable = new D3D11Drawable(new[] {
                    0, 1, 2,
                    2, 3, 0,
                    4, 5, 6,

                    7, 8, 9,
                    9, 10, 7,
                    11, 12, 13
                },
                shader,
                default(ImageBytes),
                Color4.White
            );

            {
                // X
                drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(w, 0, -w),
                    new Vector3(h, 0, -w),
                    new Vector3(h, 0, w),
                    new Vector3(w, 0, w),

                    new Vector3(h, 0, -arrow),
                    new Vector3(h+arrow, 0, 0),
                    new Vector3(h, 0, arrow),

                    new Vector3(-w, 0, w),
                    new Vector3(-w, 0, h),
                    new Vector3(w, 0, h),
                    new Vector3(w, 0, w),

                    new Vector3(-arrow, 0, h),
                    new Vector3(0, 0, h+arrow),
                    new Vector3(arrow, 0, h),
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
    }
}
