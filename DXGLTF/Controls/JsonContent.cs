using GltfScene;
using System;
using System.Reactive.Linq;
using System.Threading;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class JsonContent : DockContent
    {
        public JsonContent(Scene scene)
        {
            InitializeComponent();

            scene.SourceObservableOnCurrent
                  .Subscribe(OnSource);
        }

        void OnSource(Source source)
        {
            if (source.GLTF == null) return;

            // indent
            richTextBox1.Text = source.JSON.ToString("  ");
        }
    }
}
