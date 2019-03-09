using SharpDX;
using System.Collections.Generic;
using System;
using System.Linq;

namespace D3DPanel
{
    static class ListExtensions
    {
        public static void AddMany<T>(this List<T> list, params T[] many)
        {
            list.AddRange(many);
        }
    }

    public static class D3D11MeshFactory
    {
        public class MeshBuilder
        {
            List<int> _indices = new List<int>();
            List<Vector3> _positions = new List<Vector3>();
            List<Color4> _colors = new List<Color4>();
            SharpDX.Direct3D.PrimitiveTopology _topology;

            int GetIndex(SharpDX.Direct3D.PrimitiveTopology topology)
            {
                var i = _positions.Count;
                if (i == 0)
                {
                    _topology = topology;
                }
                else
                {
                    if (_topology != topology)
                    {
                        throw new InvalidOperationException();
                    }
                }
                return i;
            }

            public void AddLine(Vector3 p0, Vector3 p1,
               Color4 c0, Color4 c1)
            {
                var i = GetIndex(SharpDX.Direct3D.PrimitiveTopology.LineList);
                _positions.AddMany(p0, p1);
                _colors.AddMany(c0, c1);
                _indices.AddMany(i, i + 1);
            }

            public void AddTriangle(Vector3 p0, Vector3 p1, Vector3 p2,
               Color4 c0, Color4 c1, Color4 c2)
            {
                var i = GetIndex(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
                _positions.AddMany(p0, p1, p2);
                _colors.AddMany(c0, c1, c2);
                _indices.AddMany(i, i + 1, i + 2);
            }

            public void AddQuadrangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
               Color4 c0, Color4 c1, Color4 c2, Color4 c3)
            {
                var i = GetIndex(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
                _positions.AddMany(p0, p1, p2, p3);
                _colors.AddMany(c0, c1, c2, c3);
                _indices.AddMany(i, i + 1, i + 2);
                _indices.AddMany(i + 2, i + 3, i);
            }

            public void AddCube(Vector3 center, float w, float h, float d, Color4 color)
            {
                var i = GetIndex(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
                _positions.AddMany(
                    center + new Vector3(-w, -h, d),
                    center + new Vector3(w, -h, d),
                    center + new Vector3(w, h, d),
                    center + new Vector3(-w, h, d),
                    center + new Vector3(-w, -h, -d),
                    center + new Vector3(w, -h, -d),
                    center + new Vector3(w, h, -d),
                    center + new Vector3(-w, h, -d)
                    );
                _colors.AddRange(Enumerable.Repeat(color, 8));
                _indices.AddMany(0, 1, 2, 2, 3, 0);
                _indices.AddMany(1, 5, 6, 6, 2, 1);
                _indices.AddMany(5, 4, 7, 7, 6, 5);
                _indices.AddMany(4, 0, 3, 3, 7, 4);
                _indices.AddMany(3, 2, 6, 6, 7, 3);
                _indices.AddMany(4, 5, 1, 1, 0, 4);
            }

            public void AddPyramid(Matrix m, float size, Color4 color)
            {
                var i = GetIndex(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
                _positions.AddMany(
                    Vector3.TransformCoordinate(new Vector3(-size, 0, -size), m),
                    Vector3.TransformCoordinate(new Vector3(size, 0, -size), m),
                    Vector3.TransformCoordinate(new Vector3(size, 0, size), m),
                    Vector3.TransformCoordinate(new Vector3(-size, 0, size), m),
                    Vector3.TransformCoordinate(new Vector3(0, size*2, 0), m)
                    );
                _colors.AddMany(color, color, color, color, color);
                _indices.AddMany(i, i + 1, i + 2, i + 2, i + 3, i);
                _indices.AddMany(i, i + 1, i + 4);
                _indices.AddMany(i + 1, i + 2, i + 4);
                _indices.AddMany(i + 2, i + 3, i + 4);
                _indices.AddMany(i + 3, i, i + 4);
            }

            public D3D11Mesh ToMesh()
            {
                var drawable = new D3D11Mesh(_topology, _indices.ToArray());
                drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(_positions.ToArray()));
                drawable.SetAttribute(Semantics.COLOR, VertexAttribute.Create(_colors.ToArray()));
                return drawable;
            }
        }

        public static D3D11Mesh CreateTriangle(float size = 0.5f)
        {
            var builder = new MeshBuilder();
            builder.AddTriangle(
                    new Vector3(0.0f, size, 0),
                    new Vector3(size, -size, 0),
                    new Vector3(-size, -size, 0),
                    new Color4(1.0f, 0, 0, 1.0f),
                    new Color4(0, 1.0f, 0, 1.0f),
                    new Color4(0, 0, 1.0f, 1.0f)
                );
            return builder.ToMesh();
        }

        public static D3D11Mesh CreateCube(float size=0.5f)
        {
            var builder = new MeshBuilder();
            builder.AddCube(Vector3.Zero, size, size, size, Color.White);
            return builder.ToMesh();
        }

        public static D3D11Mesh CreateAxis(float w, float h)
        {
            var offset = 0.001f;
            var arrow = w * 2;
            var sub = 0.6f;
            var red = new Color4(1.0f, sub, sub, 1.0f);
            var blue = new Color4(sub, sub, 1.0f, 1.0f);

            var builder = new MeshBuilder();
            builder.AddQuadrangle(
                    new Vector3(w, offset, -w),
                    new Vector3(h, offset, -w),
                    new Vector3(h, offset, w),
                    new Vector3(w, offset, w),
                    red,
                    red,
                    red,
                    red
                );
            builder.AddTriangle(
                    new Vector3(h, offset, -arrow),
                    new Vector3(h + arrow, offset, 0),
                    new Vector3(h, offset, arrow),
                    red,
                    red,
                    red
                );
            builder.AddQuadrangle(
                    new Vector3(-w, offset, w),
                    new Vector3(-w, offset, h),
                    new Vector3(w, offset, h),
                    new Vector3(w, offset, w),
                    blue,
                    blue,
                    blue,
                    blue
                );
            builder.AddTriangle(
                    new Vector3(-arrow, offset, h),
                    new Vector3(0, offset, h + arrow),
                    new Vector3(arrow, offset, h),
                    blue,
                    blue,
                    blue
                );
            return builder.ToMesh();
        }

        public static D3D11Mesh CreateGrid(float size, int count)
        {
            var positions = new List<Vector3>();
            var colors = new List<Color4>();
            var length = size * count;

            var white = Color4.White;
            var red = new Color4(1, 0, 0, 1);
            var blue = new Color4(0, 0, 1, 1);

            var builder = new MeshBuilder();

            // vertical
            for (int i = -count; i <= count; ++i)
            {
                var color = (i == 0) ? blue : white;

                builder.AddLine(
                    new Vector3(i * size, 0, -length),
                    new Vector3(i * size, 0, length),
                    color,
                    color
                    );
            }

            // horizontal
            for (int i = -count; i <= count; ++i)
            {
                var color = (i == 0) ? red : white;

                builder.AddLine(
                    new Vector3(-length, 0, i * size),
                    new Vector3(length, 0, i * size),
                    color,
                    color
                    );
            }

            return builder.ToMesh();
        }

        public static D3D11Mesh CreateArrow(float width, float length, int axis, bool positive, Color4 color)
        {
            var builder = new MeshBuilder();
            switch (axis)
            {
                case 0:
                    {
                        var end = Vector3.UnitX * (positive ? length : -length);
                        builder.AddCube(end/2, (length / 2), width, width, color);
                        var r = Matrix.RotationZ(MathUtil.DegreesToRadians(positive ? -90 : 90));
                        builder.AddPyramid(r * Matrix.Translation(end), width * 2, color);
                    }
                    break;

                case 1:
                    {
                        var end = Vector3.UnitY * (positive ? length : -length);
                        builder.AddCube(end/2, width, (length / 2), width, color);
                        var r = positive ? Matrix.Identity : Matrix.RotationX(MathUtil.DegreesToRadians(180));
                        builder.AddPyramid(r * Matrix.Translation(end), width * 2, color);
                    }
                    break;

                case 2:
                    {
                        var end = Vector3.UnitZ * (positive ? length : -length);
                        builder.AddCube(end/2, width, width, (length / 2), color);
                        var r = Matrix.RotationX(MathUtil.DegreesToRadians(positive ? 90 : -90));
                        builder.AddPyramid(r * Matrix.Translation(end), width * 2, color);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
            return builder.ToMesh();
        }

        public static D3D11Mesh CreateQuadrangle()
        {
            var drawable = new D3D11Mesh(SharpDX.Direct3D.PrimitiveTopology.TriangleList,
                new int[] { 0, 1, 2, 2, 3, 0 });
            drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0),
                }));
            drawable.SetAttribute(Semantics.TEXCOORD, VertexAttribute.Create(new Vector2[]{
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                }));
            return drawable;
        }
    }
}
