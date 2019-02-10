using SharpDX;

namespace D3DPanel
{
    public struct Triangle
    {
        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;
    }

    public struct TriangleIntersection
    {
        public int I0;
        public int I1;
        public int I2;
        public float Distance;
    }
}
