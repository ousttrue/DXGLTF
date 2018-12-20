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
            // default triangle
            var drawable = new D3D11Drawable(new[] { 0, 1, 2 }, 
                m_shaderLoader.CreateMaterial(ShaderType.Gizmo, default(ImageBytes)));
            drawable.SetAttribute(Semantics.POSITION, VertexAttribute.Create(new Vector3[]{
                    new Vector3(0.0f, 0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0),
                }));
            m_drawables.Add(drawable);
        }

        public void SetSelection(Source source, JsonNode node)
        {
            if (source.GlTF == null)
            {
                return;
            }

            if (!node.IsValid)
            {
                return;
            }

            var p = new JsonPointer(node);
            if (p.Count == 0)
            {
                // root
                return;
            }

            Dispose();
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
                    var colorTexture = m.pbrMetallicRoughness.baseColorTexture;
                    if (colorTexture != null)
                    {
                        if (colorTexture.index != -1)
                        {
                            var texture = gltf.textures[colorTexture.index];
                            var image = gltf.images[texture.source];
                            var bytes = source.GetImageBytes(image);
                            var format = GetImageFormat(image.mimeType);
                            imageBytes = new ImageBytes(format, bytes);
                        }
                    }
                    var material = m_shaderLoader.CreateMaterial(ShaderType.Unlit, 
                        imageBytes);
                    var accessor = gltf.accessors[primitive.indices];
                    int[] indices = null;
                    if (accessor.componentType == UniGLTF.glComponentType.UNSIGNED_INT)
                    {
                        indices = gltf.GetArrayFromAccessor<int>(source.IO, primitive.indices);
                    }
                    else if (accessor.componentType == UniGLTF.glComponentType.UNSIGNED_SHORT)
                    {
                        indices = gltf.GetArrayFromAccessor<ushort>(source.IO, primitive.indices).Select(x => (int)x).ToArray();
                    }
                    else
                    {
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

                    m_drawables.Add(drawable);
                }
            }
        }

        static ImageFormat GetImageFormat(string mime)
        {
            switch(mime)
            {
                case "image/png": return ImageFormat.Png;
                case "image/jpeg": return ImageFormat.Jpeg;
            }

            Logger.Warn($"unknown mime: {mime}");
            return ImageFormat.Png;
        }

        void ShowImage(Source source, IEnumerable<UniGLTF.glTFImage> images)
        {
            var gltf = source.GlTF;
            foreach (var image in images)
            {
                var bytes = source.GetImageBytes(image);
                var format = GetImageFormat(image.mimeType);
                var material = m_shaderLoader.CreateMaterial(ShaderType.Unlit, new ImageBytes(format, bytes));
               
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

                m_drawables.Add(drawable);

                break;
            }
        }
    }
}
