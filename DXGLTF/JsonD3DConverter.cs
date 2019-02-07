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
    class Node : ITreeNode<Node, D3D11Drawable>
    {
        List<Node> m_nodes = new List<Node>();

        public bool IsValid => m_nodes != null;

        int m_parentIndex = -1;
        public int ParentIndex
        {
            get { return m_parentIndex; }
        }

        public bool HasParent => ParentIndex != -1;

        public Node Parent
        {
            get
            {
                if (!HasParent) return null;
                return m_nodes[ParentIndex];
            }
        }

        public IEnumerable<Node> Children
        {
            get
            {
                foreach (var node in m_nodes)
                {
                    if (node.ParentIndex == ValueIndex)
                    {
                        yield return node;
                    }
                }
            }
        }

        public int ValueIndex
        {
            get;
            set;
        }

        D3D11Drawable m_value;
        public D3D11Drawable Value { get { return m_value; } }
        public void SetValue(D3D11Drawable value)
        {
            m_value = value;
        }

        Matrix m_matrix = Matrix.Identity;
        public Matrix Matrix
        {
            get;
        }

        public Node(D3D11Drawable value)
        {
            m_value = value;
        }
    }


    class JsonD3DConverter : IDisposable
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        List<Node> m_drawables = new List<Node>();
        public List<Node> Drawables
        {
            get { return m_drawables; }
        }

        public void Dispose()
        {
            foreach (var x in m_drawables)
            {
                x.Value.Dispose();
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

            Dispose();

            var p = node.Pointer();
            if (p.Count == 0)
            {
                // root
                // show all
                ShowMesh(source, source.GlTF.meshes);
            }
            else
            {
                switch (p[0].ToString())
                {
                    case "meshes":
                        if (p.Count == 1)
                        {
                            ShowMesh(source, source.GlTF.meshes);
                        }
                        else
                        {
                            var index = p[1].ToInt32();
                            ShowMesh(source, new[] { source.GlTF.meshes[index] });
                        }
                        break;

                    case "images":
                        if (p.Count == 1)
                        {
                            ShowImage(source, source.GlTF.images);
                        }
                        else
                        {
                            var index = p[1].ToInt32();
                            ShowImage(source, new[] { source.GlTF.images[index] });
                        }
                        break;

                    default:
                        Logger.Debug($"selected: {p}");
                        break;
                }
            }

            m_updated.OnNext(Unit.Default);
        }

        void ShowMesh(Source source, IEnumerable<UniGLTF.glTFMesh> meshes)
        {
            var gltf = source.GlTF;
            foreach (var mesh in meshes)
            {
                foreach (var primitive in mesh.primitives)
                {
                    var m = gltf.materials[primitive.material];

                    var imageBytes = default(ImageBytes);
                    if (m.pbrMetallicRoughness != null)
                    {
                        var colorTexture = m.pbrMetallicRoughness.baseColorTexture;
                        if (colorTexture != null)
                        {
                            if (colorTexture.index != -1)
                            {
                                var texture = gltf.textures[colorTexture.index];
                                var image = gltf.images[texture.source];
                                var bytes = source.GetImageBytes(image);
                                imageBytes = new ImageBytes(bytes);
                            }
                        }
                    }
                    var material = m_shaderLoader.CreateMaterial(ShaderType.Unlit, 
                        imageBytes);
                    var accessor = gltf.accessors[primitive.indices];
                    int[] indices = null;
                    switch (accessor.componentType)
                    {
                        case UniGLTF.glComponentType.BYTE:
                            indices = gltf.GetArrayFromAccessor<byte>(source.IO, primitive.indices).Select(x => (int)x).ToArray();
                            break;

                        case UniGLTF.glComponentType.UNSIGNED_SHORT:
                            indices = gltf.GetArrayFromAccessor<ushort>(source.IO, primitive.indices).Select(x => (int)x).ToArray();
                            break;

                        case UniGLTF.glComponentType.UNSIGNED_INT:
                            indices = gltf.GetArrayFromAccessor<int>(source.IO, primitive.indices);
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    var drawable = new D3D11Drawable(indices, material);

                    var attribs = primitive.attributes;

                    {
                        var positions = gltf.GetBytesFromAccessor(source.IO, primitive.attributes.POSITION);
                        if (positions.Count == 0)
                        {
                            throw new Exception();
                        }
                        drawable.SetAttribute(Semantics.POSITION, new VertexAttribute(positions, 4 * 3));
                    }

                    if(primitive.attributes.TEXCOORD_0!=-1)
                    {
                        var uv = gltf.GetBytesFromAccessor(source.IO, primitive.attributes.TEXCOORD_0);
                        drawable.SetAttribute(Semantics.TEXCOORD, new VertexAttribute(uv, 4 * 2));
                    }

                    m_drawables.Add(new Node(drawable));
                }
            }
        }

        void ShowImage(Source source, IEnumerable<UniGLTF.glTFImage> images)
        {
            var gltf = source.GlTF;
            foreach (var image in images)
            {
                var bytes = source.GetImageBytes(image);
                var material = m_shaderLoader.CreateMaterial(
                    ShaderType.Unlit, new ImageBytes(bytes));
               
                var drawable = new D3D11Drawable(new int[] { 0, 1, 2, 2, 3, 0 }, material);
                drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0),
                }));
                drawable.SetAttribute(Semantics.TEXCOORD, VertexAttribute.Create(new Vector2[]{
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                }));

                m_drawables.Add(new Node(drawable));

                break;
            }
        }
    }
}
