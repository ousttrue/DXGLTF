using GltfScene;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DXGLTF
{
    public partial class Form1 : Form
    {
        Scene m_scene = new Scene();

        Dictionary<string, DockContent> m_contentMap = new Dictionary<string, DockContent>();
        void AddContent(string name, DockContent content, DockState state)
        {
            content.Text = name;
            content.TabText = name;
            content.Show(dockPanel1, state);
            m_contentMap.Add(name, content);
        }

        public Form1()
        {
            InitializeComponent();

            AddContent("3D", new D3DContent(m_scene), DockState.Document);
            AddContent("json", new JsonContent(m_scene), DockState.DockRight);
            AddContent("buffer view", new BufferViewContent(m_scene), DockState.DockRight);
            AddContent("accessor", new AccessorContent(m_scene), DockState.DockRight);
            AddContent("material", new MaterialContent(m_scene), DockState.DockRight);
            AddContent("node", new NodeContent(m_scene), DockState.DockLeft);
            AddContent("primitive", new PrimitiveContent(m_scene), DockState.DockRight);
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
