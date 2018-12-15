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

            scene.Json
                  .ObserveOn(SynchronizationContext.Current)
                  .Subscribe(x =>
                  {
                      richTextBox1.Text = x;
                  });
        }
    }
}
