using DXGLTF.Assets;
using DXGLTF.Drawables;
using NLog;
using NLog.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniJSON;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class Form1 : Form
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        #region Dock
        Dictionary<string, DockContent> _contentMap = new Dictionary<string, DockContent>();
        void AddContent(string name, DockContent content, DockState state)
        {
            content.Text = name;
            content.TabText = name;
            content.Show(dockPanel1, state);
            content.HideOnClose = true;
            _contentMap.Add(name, content);

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
        LoggerContent _logger;
        #endregion

        AssetLoader _loader = new AssetLoader();

        CompositeDisposable _disposable = new CompositeDisposable();

        public Form1()
        {
            InitializeComponent();

            var scene = new Scene();
            var view0 = new SceneCameraView(scene);
            var view1 = new SceneCameraView(scene);
            var splitter = new VerticalSplitter(view0, view1);
            _disposable.Add(splitter);

            // setup docks
            var hierarchy = new SceneHierarchyContent(node =>
            {
                scene.Selected = node;
            });
            AddContent("scene hierarchy", hierarchy, DockState.DockRight);

            var selected = new SelectedNodeContent(scene.SelectedObservable);
            AddContent("selected node", selected, DockState.DockRight);
            selected.DockTo(hierarchy.Pane, DockStyle.Bottom, 1);

            var d3d = new D3DContent(splitter);
            _disposable.Add(d3d);
            AddContent("selected", d3d, DockState.Document);

            var json = new JsonContent();
            AddContent("json", json, DockState.DockLeft);

            var jsonNode = new JsonNodeContent();
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
            });

            // setup scene
            _loader.SourceObservableOnCurrent.Subscribe(async source =>
            {
                json.SetAssetSource(source);
                jsonNode.SetAssetSource(source);

                //LoadAsset(x);
                if (source.GLTF == null)
                {
                    this.Text = "";
                    return;
                }

                var path = Path.GetFileName(source.Path);
                this.Text = $"[{path}] {source.GLTF.TriangleCount} tris";
                var asset = await Task.Run(() => AssetContext.Load(source));

                // update treeview
                hierarchy.SetTreeNode(asset);

                // update scene
                scene.Asset = asset;

            });
            scene.Updated
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ =>
                {
                    d3d.Invalidate();
                })
                ;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _disposable.Dispose();
            _disposable=null;
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
                _loader.Load(openFileDialog.FileName);
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

            _logger = new LoggerContent(richTextBox1);
            AddContent("logger", _logger, DockState.DockBottom);
        }
        #endregion
    }
}
