using DXGLTF.Assets;
using System;
using System.Reactive.Linq;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class JsonContent : DockContent
    {
        public JsonContent(AssetLoader scene)
        {
            InitializeComponent();

            scene.SourceObservableOnCurrent
                  .Subscribe(OnSource);
        }

        void OnSource(AssetSource source)
        {
            if (source.GLTF == null) return;

            // indent
            richTextBox1.Text = source.JSON.ToString("  ");
        }
    }
}
