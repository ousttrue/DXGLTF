using System;
using System.Drawing;
using System.Windows.Forms;


namespace DXGLTF.Controls
{
    public partial class Vector3Control : UserControl
    {
        public Vector3Control()
        {
            InitializeComponent();

            numericUpDown1.ValueChanged += NumericUpDown_ValueChanged;
            numericUpDown2.ValueChanged += NumericUpDown_ValueChanged;
            numericUpDown3.ValueChanged += NumericUpDown_ValueChanged;
        }

        bool _eventStop;
        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_eventStop)
            {
                return;
            }

            RaiseValueChanged(
                (float)numericUpDown1.Value,
                (float)numericUpDown2.Value,
                (float)numericUpDown3.Value);
        }

        void RaiseValueChanged(float x, float y, float z)
        {
            var value = new SharpDX.Vector3(x, y, z);
            _value = value;

            var handler = ValueChanged;
            if (handler == null) return;
            handler(value);
        }
        public event Action<SharpDX.Vector3> ValueChanged;

        SharpDX.Vector3 _value;
        public SharpDX.Vector3 Value
        {
            get { return _value; }
            set
            {
                if (_value.Equals(value)) return;
                _value = value;

                _eventStop = true;
                numericUpDown1.Value = (decimal)_value.X;
                numericUpDown2.Value = (decimal)_value.Y;
                numericUpDown3.Value = (decimal)_value.Z;
                _eventStop = false;
            }
        }

        private void Vector3Control_ClientSizeChanged(object sender, EventArgs e)
        {
            var w = ClientSize.Width / 3;
            var h = ClientSize.Height;

            var x = 0;

            numericUpDown1.Size = new Size(w, h);
            numericUpDown1.Location = new Point(x, 0);
            x += w;

            numericUpDown2.Size = new Size(w, h);
            numericUpDown2.Location = new Point(x, 0);
            x += w;

            numericUpDown3.Size = new Size(w, h);
            numericUpDown3.Location = new Point(x, 0);
        }
    }
}
