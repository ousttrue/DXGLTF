﻿using GltfScene;
using System;
using System.ComponentModel;
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
            scene.GltfObservableOnCurrent
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
        protected abstract void OnUpdated(glTF gltf);
    }

    class BufferViewContent : DataGridViewContentBase
    {
        public BufferViewContent(GltfScene.Scene scene) : base(scene) { }

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

        protected override void OnUpdated(glTF gltf)
        {
            m_items.Clear();
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
        public AccessorContent(GltfScene.Scene scene) : base(scene) { }

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

        protected override void OnUpdated(glTF gltf)
        {
            m_items.Clear();
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

    class MaterialContent : DataGridViewContentBase
    {
        public MaterialContent(GltfScene.Scene scene) : base(scene) { }

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

        protected override void OnUpdated(glTF gltf)
        {
            m_items.Clear();
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
