using DXGLTFContent;
using GltfScene;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;


namespace DXGLTF
{
    public partial class Form1 : Form
    {
        Scene m_scene = new Scene();

        D3DContent m_d3dContent;
        JsonContent m_jsonContent;

        public Form1()
        {
            InitializeComponent();

            m_d3dContent = new D3DContent();
            m_d3dContent.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.Document);

            m_jsonContent = new JsonContent(m_scene);
            m_jsonContent.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockLeft);
        }

        struct FileDialogFilter
        {
            public string Label;
            public string[] Extensions;
            public FileDialogFilter(string label, params string[] extensions)
            {
                Label = label;
                Extensions = extensions;
            }
            public override string ToString()
            {
                var joined = string.Join(";", Extensions.Select(x => "*." + x));
                return String.Format("{0}({1})|{2}", Label, joined, joined);
            }
        }
        static FileDialogFilter[] filters = new FileDialogFilter[]
        {
            new FileDialogFilter("glTF files", new string[]{"gltf","glb"}),
            new FileDialogFilter("All files", new string[]{"*"}),
        };

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = string.Join("|", filters.Select(x => x.ToString()));
                openFileDialog.FilterIndex = 0;
                //openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                OpenFile(openFileDialog.FileName);
            }
        }

        void OpenFile(string file)
        {
            m_scene.Load(file);
        }
    }
}
