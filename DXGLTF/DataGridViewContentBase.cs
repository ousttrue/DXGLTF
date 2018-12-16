using GltfScene;
using System;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using UniGLTF;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public abstract partial class DataGridViewContentBase : DockContent
    {
        public DataGridViewContentBase(Scene scene)
        {
            InitializeComponent();
            dataGridView1.DataSource = DataSource;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            scene.Gltf
                    .Where(x => x != null)
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(OnUpdated);
        }

        protected void AutoResizeColumns()
        {
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        protected abstract object DataSource { get; }
        protected abstract void OnUpdated(glTF gltf);
    }
}
