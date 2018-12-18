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

        public D3DContent(Scene scene)
        {
            InitializeComponent();

            scene.GltfObservableOnCurrent.Subscribe(x =>
                OnSceneLoaded(x, UniGLTF.FolderIO.FromFile(scene.LoadPath.Value)));

            m_drawables.Add(new D3D11Drawable(new[] { 0, 1, 2 }, CreateMaterial(null),
                new Vector3[]
                {
                    new Vector3(0.0f, 0.5f, 0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                },
                null,
                null
                ));
        }

        D3D11Shader CreateMaterial(UniGLTF.glTFMaterial material)
        {
            string vsPath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "shader.hlsl");
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
            if (gltf == null)
            {
                return;
            }
            m_drawables.Clear();

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
        }

        private void D3DContent_Paint(object sender, PaintEventArgs e)
        {
            m_renderer.Begin(Handle);
            foreach (var x in m_drawables)
            {
                x.Draw(m_renderer);
            }
            m_renderer.End();
        }

        private void D3DContent_SizeChanged(object sender, EventArgs e)
        {
            m_renderer.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }
    }
}
