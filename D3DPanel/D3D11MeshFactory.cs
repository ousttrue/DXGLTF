using SharpDX;
using System.Collections.Generic;
using System;


namespace D3DPanel
{
    public static class D3D11MeshFactory
    {
        public class MeshBuilder
        {
            List<int> _indices = new List<int>();
            List<Vector3> _positions = new List<Vector3>();
            List<Color4> _colors = new List<Color4>();
            SharpDX.Direct3D.PrimitiveTopology _topology;

            public void AddLine(Vector3 p0, Vector3 p1,
               Color4 c0, Color4 c1)
            {
                var i = _positions.Count;
                if (i == 0)
                {
                    _topology = SharpDX.Direct3D.PrimitiveTopology.LineList;
                }
                else
                {
                    if (_topology != SharpDX.Direct3D.PrimitiveTopology.LineList)
                    {
                        throw new InvalidOperationException();
                    }
                }

                _positions.Add(p0);
                _positions.Add(p1);
                _colors.Add(c0);
                _colors.Add(c1);
                _indices.Add(i);
                _indices.Add(i + 1);
            }

            public void AddTriangle(Vector3 p0, Vector3 p1, Vector3 p2,
               Color4 c0, Color4 c1, Color4 c2)
            {
                var i = _positions.Count;
                if (i == 0)
                {
                    _topology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                }
                else
                {
                    if (_topology != SharpDX.Direct3D.PrimitiveTopology.TriangleList)
                    {
                        throw new InvalidOperationException();
                    }
                }
                _positions.Add(p0);
                _positions.Add(p1);
                _positions.Add(p2);
                _colors.Add(c0);
                _colors.Add(c1);
                _colors.Add(c2);
                _indices.Add(i);
                _indices.Add(i + 1);
                _indices.Add(i + 2);
            }

            public void AddQuadrangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
               Color4 c0, Color4 c1, Color4 c2, Color4 c3)
            {
                var i = _positions.Count;
                if (i == 0)
                {
                    _topology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                }
                else
                {
                    if (_topology != SharpDX.Direct3D.PrimitiveTopology.TriangleList)
                    {
                        throw new InvalidOperationException();
                    }
                }
                _positions.Add(p0);
                _positions.Add(p1);
                _positions.Add(p2);
                _positions.Add(p3);
                _colors.Add(c0);
                _colors.Add(c1);
                _colors.Add(c2);
                _colors.Add(c3);
                _indices.Add(i);
                _indices.Add(i + 1);
                _indices.Add(i + 2);
                _indices.Add(i + 2);
                _indices.Add(i + 3);
                _indices.Add(i);
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

        public static D3D11Mesh CreateAxis(float w, float h)
        {
            var offset = 0.001f;
            var arrow = w * 2;
            var red = new Color4(1.0f, 0, 0, 1.0f);
            var blue = new Color4(0, 0, 1.0f, 1.0f);

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

        static Vector3 GetX(int axis, bool positive)
        {
            switch (axis)
            {
                case 0: return Vector3.UnitY;
                case 1: return Vector3.UnitZ;
                case 2: return Vector3.UnitX;
                default: throw new NotImplementedException();
            }
        }

        static Vector3 GetY(int axis, bool positive)
        {
            switch (axis)
            {
                case 0: return Vector3.UnitZ;
                case 1: return Vector3.UnitX;
                case 2: return Vector3.UnitY;
                default: throw new NotImplementedException();
            }
        }

        static Vector3 GetZ(int axis, bool positive)
        {
            switch (axis)
            {
                case 0: return positive ? Vector3.UnitX : -Vector3.UnitX;
                case 1: return positive ? Vector3.UnitY : -Vector3.UnitY;
                case 2: return positive ? Vector3.UnitZ : -Vector3.UnitZ;
                default: throw new NotImplementedException();
            }
        }

        public static D3D11Mesh CreateArrow(float width, float length, int axis, bool positive)
        {
            var x = GetX(axis, positive);
            var y = GetY(axis, positive);
            var z = GetZ(axis, positive);

            var builder = new MeshBuilder();
            return builder.ToMesh();
        }
    }
}
