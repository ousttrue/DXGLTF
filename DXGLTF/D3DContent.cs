using D3DPanel;
using GltfScene;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTFContent
{
    public partial class D3DContent : DockContent
    {
        D3D11Renderer m_renderer = new D3D11Renderer();
        List<D3D11Drawable> m_drawables = new List<D3D11Drawable>();
        Camera m_camera = new Camera
        {
            View = Matrix.Identity,
        };

        public D3DContent(Scene scene)
        {
            InitializeComponent();

            scene.GltfObservableOnCurrent.Subscribe(x =>
            {
                OnSceneLoaded(x.Item1, x.Item2);
            });

            m_drawables.Add(new D3D11Drawable(new[] { 0, 1, 2 }, CreateMaterial(null),
                new Vector3[]{
                    new Vector3(0.0f, 0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0),
                },
                null,
                null
                ));
        }

        D3D11Shader CreateMaterial(UniGLTF.glTFMaterial material)
        {
            string vsPath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "shaders/shader.hlsl");
            string psPath = vsPath;
            var vsSource = File.ReadAllText(vsPath, Encoding.UTF8);
            var psSource = default(string);
            if (vsPath == psPath)
            {
                psSource = vsSource;
            }
            else
            {
                psSource = File.ReadAllText(psPath, Encoding.UTF8);
            }
            return new D3D11Shader(vsSource, psSource);
        }

        void OnSceneLoaded(UniGLTF.glTF gltf, UniGLTF.IBufferIO io)
        {
            foreach (var x in m_drawables)
            {
                x.Dispose();
            }
            m_drawables.Clear();
            if (gltf == null)
            {
                return;
            }

            foreach (var mesh in gltf.meshes)
            {
                foreach (var primitive in mesh.primitives)
                {
                    var i = gltf.accessors[primitive.indices];
                    int[] indices = null;
                    if (i.componentType == UniGLTF.glComponentType.UNSIGNED_INT)
                    {
                        indices = gltf.GetArrayFromAccessor<int>(io, primitive.indices);
                    }
                    else if (i.componentType == UniGLTF.glComponentType.UNSIGNED_SHORT)
                    {
                        indices = gltf.GetArrayFromAccessor<ushort>(io, primitive.indices).Select(x => (int)x).ToArray();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    var positions = gltf.GetArrayFromAccessor<Vector3>(io, primitive.attributes.POSITION);
                    var material = CreateMaterial(gltf.materials[primitive.material]);
                    var drawable = new D3D11Drawable(indices, material, positions, null, null);
                    m_drawables.Add(drawable);
                }
            }
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        private void D3DContent_Paint(object sender, PaintEventArgs e)
        {
            m_camera.Update();
            m_renderer.Begin(Handle, m_camera);
            foreach (var x in m_drawables)
            {
                x.Draw(m_renderer);
            }
            m_renderer.End();
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            m_renderer.Resize(ClientSize.Width, ClientSize.Height);
            m_camera.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }

        int m_mouseX;
        int m_mouseY;
        private void D3DContent_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_mouseX != 0 && m_mouseY != 0)
            {
                var deltaX = e.X - m_mouseX;
                var deltaY = e.Y - m_mouseY;

                if (m_rightDown)
                {
                    m_camera.YawPitch(deltaX, deltaY);
                    Invalidate();
                }

                if (m_middleDown)
                {
                    m_camera.Shift(deltaX, deltaY);
                    Invalidate();
                }
            }
            m_mouseX = e.X;
            m_mouseY = e.Y;
        }

        bool m_leftDown;
        bool m_middleDown;
        bool m_rightDown;
        private void D3DContent_MouseDown(object sender, MouseEventArgs e)
        {
            Focus();

            switch (e.Button)
            {
                case MouseButtons.Left:
                    m_leftDown = true;
                    Capture = true;
                    break;

                case MouseButtons.Middle:
                    m_middleDown = true;
                    Capture = true;
                    break;

                case MouseButtons.Right:
                    m_rightDown = true;
                    Capture = true;
                    break;
            }
        }

        private void D3DContent_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    m_leftDown = false;
                    break;

                case MouseButtons.Middle:
                    m_middleDown = false;
                    break;

                case MouseButtons.Right:
                    m_rightDown = false;
                    break;
            }

            if (!m_leftDown
                && !m_middleDown
                && !m_rightDown)
            {
                Capture = false;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            m_camera.Dolly(e.Delta);
            Invalidate();
        }
    }
}
