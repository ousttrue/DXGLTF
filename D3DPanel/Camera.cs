using SharpDX;


namespace D3DPanel
{
    public class Camera
    {
        const float ToRadians = (float)(System.Math.PI / 180);

        int m_screenWidth;
        int m_screenHeight;
        public void Resize(int w, int h)
        {
            m_screenWidth = w;
            m_screenHeight = h;
            AspectRatio = (float)w / h;
        }

        const float ANGLE = 0.008f;
        public Matrix View;
        public float Yaw;
        public float Pitch;
        public void YawPitch(int dx, int dy)
        {
            Yaw += dx * ANGLE;
            Pitch += dy * ANGLE;
        }

        const float SHIFT = 1.0f;
        public float ShiftX;
        public float ShiftY;
        public void Shift(int dx, int dy)
        {
            ShiftX += ((float)dx / m_screenWidth) * Distance * SHIFT;
            ShiftY += ((float)dy / m_screenWidth) * Distance * SHIFT;
        }
        public float Distance = 5;
        public void Dolly(int delta)
        {
            if (delta < 0)
            {
                Distance *= 1.1f;
            }
            else if (delta > 0)
            {
                Distance *= 0.9f;
            }
        }

        public Matrix Projection;
        public float FovY = 30.0f * ToRadians;
        public float AspectRatio = 1.0f;
        public float ZNear = 0.1f;
        public float ZFar = 500.0f;

        public Matrix ViewProjection;

        public void Update()
        {
            View = Matrix.RotationY(Yaw) * Matrix.RotationX(Pitch) * Matrix.Translation(ShiftX, -ShiftY, -Distance);
            //View = Matrix.RotationYawPitchRoll(Yaw, Pitch, 0) * Matrix.Translation(ShiftX, -ShiftY, -Distance);
            Projection = Matrix.PerspectiveFovRH(FovY, AspectRatio, ZNear, ZFar);
            ViewProjection = View * Projection;
        }
    }
}
