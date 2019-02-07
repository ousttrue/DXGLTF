using D3DPanel;
using DXGLTF.nodes;
using GltfScene;
using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Linq;
using UniJSON;


namespace DXGLTF
{
    class JsonD3DConverter : IDisposable
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        List<Node> m_drawables = new List<Node>();
        public IEnumerable<Node> Drawables
        {
            get { return m_drawables; }
        }

        public void Dispose()
        {
            ClearDrawables();
        }

        void ClearDrawables()
        {
            foreach (var x in m_drawables)
            {
                x.Dispose();
            }
            m_drawables.Clear();
        }

        ShaderLoader m_shaderLoader = new ShaderLoader();

        Subject<Unit> m_updated = new Subject<Unit>();
        public IObservable<Unit> UpdatedObservable
        {
            get { return m_updated; }
        }

        IVisualizer[] _visualizers = new IVisualizer[]
            {
                new MeshVisualizer(),
                new ImageVisualizer(),
            };

        public JsonD3DConverter()
        {
            // default triangle
            var drawable = new D3D11Drawable(new[] { 0, 1, 2 },
                m_shaderLoader.CreateMaterial(ShaderType.Gizmo, default(ImageBytes)));
            drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(0.0f, 0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0),
                }));
            m_drawables.Add(new Node(drawable));
        }

        public void SetSelection(Source source, ListTreeNode<JsonValue> node)
        {
            if (source.GlTF == null)
            {
                return;
            }

            if (!node.IsValid)
            {
                return;
            }

            ClearDrawables();

            var p = node.Pointer();
            if (p.Count == 0)
            {
                // root
                return;
            }
            Logger.Debug($"selected: {p}");

            foreach (var v in _visualizers)
            {
                if (v.BuildNode(source, p, m_shaderLoader, m_drawables))
                {
                    break;
                }
            }

            m_updated.OnNext(Unit.Default);
        }
    }
}
