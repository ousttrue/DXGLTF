using GltfScene;
using NLog;
using NLog.Windows.Forms;
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
        static Logger Logger = LogManager.GetCurrentClassLogger();

        Scene m_scene = new Scene();

        Dictionary<string, DockContent> m_contentMap = new Dictionary<string, DockContent>();
        void AddContent(string name, DockContent content, DockState state)
        {
            content.Text = name;
            content.TabText = name;
            content.Show(dockPanel1, state);
            m_contentMap.Add(name, content);
        }

        LoggerContent m_logger;
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

            AddContent("jsonnode", new JsonNodeContent(m_scene), DockState.DockLeft);
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
            new FileDialogFilter("glTF files", new string[]{"gltf","glb", "vrm"}),
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
            Logger.Info($"OpenFile: {file}");

            m_scene.Load(file);
        }

        public RichTextBox RichTextBox
        {
            get;
            private set;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RichTextBoxTarget target = new RichTextBoxTarget();
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
            target.ControlName = richTextBox1.Name;
            target.FormName = this.Name;
            target.UseDefaultRowColoringRules = true;
            target.AutoScroll = true;

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            /*
            Logger logger = LogManager.GetLogger("Example");
            logger.Trace("trace log message");
            logger.Debug("debug log message");
            logger.Info("info log message");
            logger.Warn("warn log message");
            logger.Error("error log message");
            logger.Fatal("fatal log message");

            RichTextBoxTarget.ReInitializeAllTextboxes(this); //more on this later
            //RichTextBoxTarget.GetTargetByControl(richTextBox1).LinkClicked += LoggerContent_LinkClicked;
            */

            m_logger = new LoggerContent(richTextBox1);
            AddContent("logger", m_logger, DockState.DockBottom);
        }
    }
}
