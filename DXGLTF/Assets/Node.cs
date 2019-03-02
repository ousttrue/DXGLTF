using D3DPanel;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DXGLTF.Assets
{
    public class Node : IDisposable
    {
        public bool IsValid => true;

        public Node Parent
        {
            get;
            private set;
        }
        List<Node> _children = new List<Node>();
        public IEnumerable<Node> Children
        {
            get
            {
                return _children;
            }
        }
        public void AddChild(Node child)
        {
            _children.Add(child);
            child.Parent = this;
        }

        public Mesh Mesh
        {
            get;
            set;
        }

        public void Dispose()
        {
            if (Mesh != null)
            {
                Mesh.Dispose();
                Mesh = null;
            }

            foreach (var x in _children)
            {
                x.Dispose();
            }
            _children.Clear();
        }

        #region Local
        Matrix _localMatrix = Matrix.Identity;
        public Matrix LocalMatrix
        {
            get { return _localMatrix; }
            set
            {
                _localMatrix = value;
                Quaternion q;
                _localMatrix.Decompose(out _localScale, out q, out _localPosition);
                _localEuler = ToEulerAngle(q);
            }
        }

        static double CopySign(double a, double b)
        {
            if (b < 0)
            {
                return -Math.Abs(a);
            }
            else
            {
                return Math.Abs(a);
            }
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        static Vector3 ToEulerAngle(Quaternion q)
        {
            // roll (x-axis rotation)
            var sinr_cosp = +2.0 * (q.W * q.X + q.Y * q.Z);
            var cosr_cosp = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            var roll = Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            var sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
            var pitch = (Math.Abs(sinp) >= 1)
                ? CopySign(Math.PI / 2, sinp) // use 90 degrees if out of range
                : Math.Asin(sinp)
                ;

            // yaw (z-axis rotation)
            double siny_cosp = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            var yaw = Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3((float)roll, (float)pitch, (float)yaw);
        }

        void CalcMatrix()
        {
            _localMatrix = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, _localScale,
                Vector3.Zero, Quaternion.RotationYawPitchRoll(_localEuler.Y, _localEuler.X, _localEuler.Z),
                _localPosition);
        }

        Vector3 _localPosition;
        public Vector3 LocalPosition
        {
            get { return _localPosition; }
            set
            {
                _localPosition = value;
                CalcMatrix();
            }
        }

        Vector3 _localEuler;
        public Vector3 LocalEuler
        {
            get { return _localEuler; }
            set
            {
                _localEuler = value;
                CalcMatrix();
            }
        }

        Vector3 _localScale;
        public Vector3 LocalScale
        {
            get { return _localScale; }
            set
            {
                _localScale = value;
                CalcMatrix();
            }
        }
        #endregion

        Matrix _worldMatrix = Matrix.Identity;
        public Matrix WorldMatrix
        {
            get { return _worldMatrix; }
            set
            {
                if (_worldMatrix == value)
                {
                    return;
                }
                _worldMatrix = value;
            }
        }

        public int Index
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return base.ToString();
            }
            else
            {
                return $"{Name}";
            }
        }

        public Node(string name, int index = -1)
        {
            Name = name;
        }

        public Node(D3D11Shader shader, D3D11Mesh mesh) : this(new D3D11Material(shader.Name, shader), mesh)
        {
        }

        public Node(D3D11Material material, D3D11Mesh mesh)
        {
            Mesh = new Mesh(new Submesh(material, mesh));
        }

        public IEnumerable<SubmeshIntersection> Intersect(Ray ray)
        {
            if (Mesh == null)
            {
                return Enumerable.Empty<SubmeshIntersection>();
            }

            return Mesh.Intersect(WorldMatrix, ray);
        }

        public void Update(Matrix accumulated)
        {
            WorldMatrix = LocalMatrix * accumulated;
            foreach (var child in Children)
            {
                child.Update(WorldMatrix);
            }
        }

        /// <summary>
        /// 描画しながらついでにWorldMatrixを更新する
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="camera"></param>
        /// <param name="accumulated"></param>
        public void Draw(D3D11Renderer renderer, Camera camera)
        {
            if (Mesh != null)
            {
                Mesh.Draw(renderer, camera, WorldMatrix);
            }

            foreach (var child in Children)
            {
                child.Draw(renderer, camera);
            }
        }
    }
}
