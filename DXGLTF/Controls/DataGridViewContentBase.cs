using DXGLTF.Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Forms;
using UniGLTF;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public abstract partial class DataGridViewContentBase : DockContent
    {
        public DataGridViewContentBase(AssetLoader scene)
        {
            InitializeComponent();
            dataGridView1.DataSource = DataSource;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            scene.SourceObservableOnCurrent
                    .Subscribe(x =>
                    {
                        OnUpdated(x);
                        dataGridView1.Refresh();
                    });
        }

        protected void AutoResizeColumns()
        {
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        protected abstract object DataSource { get; }
        protected abstract void OnUpdated(AssetSource source);
    }

    class BufferViewContent : DataGridViewContentBase
    {
        public BufferViewContent(AssetLoader scene) : base(scene) { }

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
        protected override object DataSource => m_items;

        protected override void OnUpdated(AssetSource source)
        {
            m_items.Clear();
            var gltf = source.GLTF;
            if (gltf == null)
            {
                return;
            }

            foreach (var x in gltf.bufferViews)
            {
                m_items.Add(new Item(x));
            }
            AutoResizeColumns();
        }
    }

    class AccessorContent : DataGridViewContentBase
    {
        public AccessorContent(AssetLoader scene) : base(scene) { }

        class Item
        {
            public string Name { get; private set; }
            public int View { get; private set; }
            public int Offset { get; private set; }
            public int Count { get; private set; }
            public string Type { get; private set; }

            public Item(glTFAccessor src)
            {
                Name = src.name;
                View = src.bufferView;
                Offset = src.byteOffset;
                Count = src.count;
                Type = src.type;
            }
        }
        BindingList<Item> m_items = new BindingList<Item>();
        protected override object DataSource => m_items;

        protected override void OnUpdated(AssetSource source)
        {
            m_items.Clear();
            var gltf = source.GLTF;
            if (gltf == null)
            {
                return;
            }

            foreach (var x in gltf.accessors)
            {
                m_items.Add(new Item(x));
            }
            AutoResizeColumns();
        }
    }

    class PrimitiveContent : DataGridViewContentBase
    {
        public PrimitiveContent(AssetLoader scene) : base(scene) { }

        class Item
        {
            public string Name { get; private set; }
            public int Material { get; private set; }
            public int Mode { get; private set; }
            public string Attributes { get; private set; }
            public int Indices { get; private set; }

            public Item(string name, glTFPrimitives primitives)
            {
                Name = name;

                Material = primitives.material;
                Mode = primitives.mode;
                if (primitives.attributes != null)
                {
                    var sb = new List<string>();
                    if (primitives.attributes.POSITION != -1)
                    {
                        sb.Add($"POS={primitives.attributes.POSITION}");
                    }
                    if (primitives.attributes.NORMAL != -1)
                    {
                        sb.Add($"NOM={primitives.attributes.NORMAL}");
                    }
                    if (primitives.attributes.TEXCOORD_0 != -1)
                    {
                        sb.Add($"TEX={primitives.attributes.TEXCOORD_0}");
                    }
                    if (primitives.attributes.COLOR_0 != -1)
                    {
                        sb.Add($"COL={primitives.attributes.COLOR_0}");
                    }
                    if (primitives.attributes.JOINTS_0 != -1)
                    {
                        sb.Add($"JOT={primitives.attributes.JOINTS_0}");
                    }
                    if (primitives.attributes.WEIGHTS_0 != -1)
                    {
                        sb.Add($"WGT={primitives.attributes.WEIGHTS_0}");
                    }
                    Attributes = string.Join(",", sb);
                }
                Indices = primitives.indices;
            }
        }
        BindingList<Item> m_items = new BindingList<Item>();
        protected override object DataSource => m_items;

        protected override void OnUpdated(AssetSource source)
        {
            m_items.Clear();
            var gltf = source.GLTF;
            if (gltf == null)
            {
                return;
            }

            foreach (var x in gltf.meshes)
            {
                for (int i = 0; i < x.primitives.Count; ++i)
                {
                    m_items.Add(new Item($"{x.name}[{i}]", x.primitives[i]));
                }
            }
            AutoResizeColumns();
        }
    }

    class MaterialContent : DataGridViewContentBase
    {
        public MaterialContent(AssetLoader scene) : base(scene) { }

        class Item
        {
            public string Name { get; private set; }
            public Item(glTFMaterial src)
            {
                Name = src.name;
            }
        }
        BindingList<Item> m_items = new BindingList<Item>();
        protected override object DataSource => m_items;

        protected override void OnUpdated(AssetSource source)
        {
            m_items.Clear();
            var gltf = source.GLTF;
            if (gltf == null)
            {
                return;
            }

            foreach (var x in gltf.materials)
            {
                m_items.Add(new Item(x));
            }
            AutoResizeColumns();
        }
    }

}
