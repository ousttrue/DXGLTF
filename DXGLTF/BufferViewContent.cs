using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using GltfScene;
using System.Reactive.Linq;
using System.Threading;
using UniGLTF;

namespace DXGLTF
{
    public partial class BufferViewContent : DockContent
    {
        class Item
        {
            public string Name { get; private set; }
            public int Buffer { get; private set; }
            public int Offset { get; private set; }
            public int Length { get; private set; }
            public int Stride { get; private set; }

            public Item(glTFBufferView src)
            {
                Name = src.name;
                Buffer = src.buffer;
                Offset = src.byteOffset;
                Length = src.byteLength;
                Stride = src.byteStride;
            }
        }

        BindingList<Item> m_items = new BindingList<Item>();
        public BufferViewContent(Scene scene)
        {
            InitializeComponent();

            dataGridView1.DataSource = m_items;

            scene.Gltf
                    .Where(x => x != null)
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(x =>
                    {
                        m_items.Clear();
                        foreach (var v in x.bufferViews)
                        {
                            m_items.Add(new Item(v));
                        }
                    });
        }
    }
}
