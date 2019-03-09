using DXGLTF.Assets;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class JsonContent : DockContent
    {
        public JsonContent()
        {
            InitializeComponent();
        }

        public void SetAssetSource(AssetSource source)
        {
            if (source.GLTF == null) return;

            // indent
            richTextBox1.Text = source.JSON.ToString("  ");
        }
    }
}
