﻿using GltfScene;
using NLog;
using NLog.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UniJSON;
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
            content.HideOnClose = true;
            m_contentMap.Add(name, content);

            var cb = new ToolStripMenuItem();
            cb.Checked = true;
            cb.CheckOnClick = true;
            cb.Text = name;
            cb.Name = "chk_" + name;
            cb.CheckedChanged += (o, e) =>
            {
                var check = ((ToolStripMenuItem)o).Checked;
                if (check == content.Visible)
                {
                    return;
                }

                if (check)
                {
                    //Console.WriteLine(string.Format("checked: {0}", name));
                    content.Show();
                }
                else
                {
                    //Console.WriteLine(string.Format("unchecked: {0}", name));
                    content.Hide();
                }
            };

            viewToolStripMenuItem.DropDownItems.Add(cb);

            content.VisibleChanged += (o, e) =>
              {
                  //Console.WriteLine(string.Format("visible: {0}.{1}", name, content.Visible));
                  cb.Checked = content.Visible;
              };
        }

        SceneHierarchy m_hierarchy;
        D3DContent m_d3d;

        LoggerContent m_logger;

        SelectedNodeContent m_selected;

        public Form1()
        {
            InitializeComponent();

            m_scene.SourceObservableOnCurrent.Subscribe(x =>
            {
                if (x.GLTF != null)
                {
                    var path = Path.GetFileName(x.Path);
                    this.Text = $"[{path}] {x.GLTF.TriangleCount} tris";
                }
                else
                {
                    this.Text = "";
                }
            });

            m_hierarchy = new SceneHierarchy(m_scene);
            AddContent("scene hierarchy", m_hierarchy, DockState.DockRight);

            m_selected = new SelectedNodeContent(m_hierarchy);
            AddContent("selected node", m_selected, DockState.DockRight);
            m_selected.DockTo(m_hierarchy.Pane, DockStyle.Bottom, 1);

            m_d3d = new D3DContent(m_hierarchy);
            AddContent("selected", m_d3d, DockState.Document);

            AddContent("json", new JsonContent(m_scene), DockState.DockLeft);

            var jsonNode = new JsonNodeContent(m_scene);
            AddContent("jsonnode", jsonNode, DockState.DockLeft);
            jsonNode.Selected.Subscribe(x =>
            {
                if (x.IsValid)
                {
                    var p = x.Pointer();
                    toolStripStatusLabel1.Text = $"{p}";
                }
                else
                {
                    toolStripStatusLabel1.Text ="";
                }
                //m_d3d.SetSelection(jsonNode.Source, x);
            });
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_hierarchy.Shutdown();
            m_d3d.Shutdown();
        }

        #region FileDialog
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
            new FileDialogFilter("glTF files", new string[]{"gltf","glb", "vrm", "vci", "zip"}),
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
                m_scene.Load(openFileDialog.FileName);
            }
        }
        #endregion

        #region Logger
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
        #endregion
    }
}
