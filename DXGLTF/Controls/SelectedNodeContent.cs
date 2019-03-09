using DXGLTF.Assets;
using SharpDX;
using System;
using System.Reactive.Linq;
using System.Threading;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class SelectedNodeContent : DockContent
    {
        public SelectedNodeContent(SceneHierarchyContent hierarchy)
        {
            InitializeComponent();

            hierarchy.SelectedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(SetNode)
                ;

            translation.ValueChanged += x => LocalPosition = x;
            euler.ValueChanged += x => LocalEuler = x;
            scale.ValueChanged += x => LocalScale = x;
        }

        #region FromUI
        Vector3 LocalPosition
        {
            set
            {
                _localPosition = value;
                CalcMatrix();
            }
        }

        Vector3 LocalEuler
        {
            set
            {
                _localEuler = value;
                CalcMatrix();
            }
        }

        Vector3 LocalScale
        {
            set
            {
                _localScale = value;
                CalcMatrix();
            }
        }

        void CalcMatrix()
        {
            if (_node == null) return;
            _node.LocalMatrix = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, _localScale,
                Vector3.Zero, Quaternion.RotationYawPitchRoll(_localEuler.Y, _localEuler.X, _localEuler.Z),
                _localPosition);
        }
        #endregion

        #region ToUI
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
        Vector3 _localScale;
        Vector3 _localEuler;
        Vector3 _localPosition;
        void SetMatrixToUI(Matrix x)
        {
            Quaternion q;
            x.Decompose(out _localScale, out q, out _localPosition);
            _localEuler = ToEulerAngle(q);

            translation.Value = _localPosition;
            euler.Value = _localEuler;
            scale.Value = _localScale;
        }
        #endregion

        Node _node;
        IDisposable _subscription;
        void SetNode(Node node)
        {
            if (_node == node) return;

            if (_subscription != null)
            {
                // clear event
                _subscription.Dispose();
                _subscription = null;
            }

            _node = node;
            if (_node != null)
            {
                _subscription = _node.LocalMatrixObservable.Subscribe(SetMatrixToUI);
                translation.Enabled = true;
                euler.Enabled = true;
                scale.Enabled = true;
            }
            else
            {
                translation.Enabled = false;
                euler.Enabled = false;
                scale.Enabled = false;
            }
        }
    }
}
