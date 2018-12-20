using D3DPanel;
using GltfScene;
using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UniJSON;


namespace DXGLTF
{
    class JsonD3DConverter : IDisposable
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        List<D3D11Drawable> m_drawables = new List<D3D11Drawable>();
        public void Draw(D3D11Renderer renderer)
        {
            foreach (var x in m_drawables)
            {
                x.Draw(renderer);
            }
        }
        public void Dispose()
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

        public JsonD3DConverter()
        {
            var drawable = new D3D11Drawable(new[] { 0, 1, 2 }, m_shaderLoader.CreateMaterial(ShaderType.Gizmo));
            drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(0.0f, 0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0),
                }));
            m_drawables.Add(drawable);
        }

        public void SetSelection(Source source, JsonNode node)
        {
            if (!node.IsValid)
            {
                Logger.Debug($"selected: none");
                return;
            }

            var p = new JsonPointer(node);
            if (p.Count == 0)
            {
                // root
                return;
            }

            switch (p[0].ToString())
            {
                case "meshes":
                    ShowMesh(source, node, p);
                    break;

                default:
                    Logger.Debug($"selected: {p}");
                    break;
            }
            m_updated.OnNext(Unit.Default);
        }

        void ShowMesh(Source source, JsonNode node, JsonPointer p)
        {
            Dispose();
            var gltf = source.GlTF;
            if (gltf == null)
            {
                return;
            }

            if (p.Count == 1)
            {
                // meshes
                ShowMesh(source, source.GlTF.meshes);
            }
            else
            {
                var index = p[1].ToInt32();
                ShowMesh(source, new[] { source.GlTF.meshes[index] });
            }
        }

        void ShowMesh(Source source, IEnumerable<UniGLTF.glTFMesh> meshes)
        {
            var gltf = source.GlTF;
            foreach (var mesh in meshes)
            {
                foreach (var primitive in mesh.primitives)
                {
                    var i = gltf.accessors[primitive.indices];
                    int[] indices = null;
                    if (i.componentType == UniGLTF.glComponentType.UNSIGNED_INT)
                    {
                        indices = gltf.GetArrayFromAccessor<int>(source.IO, primitive.indices);
                    }
                    else if (i.componentType == UniGLTF.glComponentType.UNSIGNED_SHORT)
                    {
                        indices = gltf.GetArrayFromAccessor<ushort>(source.IO, primitive.indices).Select(x => (int)x).ToArray();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    var material = m_shaderLoader.CreateMaterial(ShaderType.Unlit);
                    var drawable = new D3D11Drawable(indices, material);

                    var attribs = primitive.attributes;

                    var positions = gltf.GetBytesFromAccessor(source.IO, primitive.attributes.POSITION);
                    if (positions.Count == 0)
                    {
                        throw new Exception();
                    }
                    drawable.SetAttribute(Semantics.POSITION, new VertexAttribute(positions, 4 * 3));

                    m_drawables.Add(drawable);
                }
            }
        }
    }
}
