using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXGLTF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
            Console.WriteLine(file);
        }
    }
}
