using System;
using System.Reactive.Linq;
using System.Threading;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class SelectedNodeContent : DockContent
    {
        public SelectedNodeContent(SceneHierarchy hierarchy)
        {
            InitializeComponent();

            hierarchy.SelectedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(x =>
                {
                    propertyGrid1.SelectedObject = x;
                });
        }
    }
}
