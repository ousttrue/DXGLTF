using D3DPanel;
using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DXGLTF.Assets
{
    public class AssetContext : IDisposable
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        AssetSource _source;
        AssetContext(AssetSource source)
        {
            _source = source;
        }

        List<ImageBytes> _textureImages = new List<ImageBytes>();
        List<D3D11Material> _materials = new List<D3D11Material>();
        List<Mesh> _meshes = new List<Mesh>();

        Node[] _nodes;
        public Node[] Nodes
        {
            get { return _nodes; }
        }

        public IEnumerable<Node> Roots
        {
            get
            {
                return Nodes.Where(x => !Nodes.Any(y => y.Children.Contains(x)));
            }
        }

        List<Skin> _skins = new List<Skin>();
        public void UpdateSkins()
        {
            foreach(var skin in _skins)
            {
                skin.Update(_nodes);
            }
        }

        public void Dispose()
        {
        }

        public static AssetContext Load(AssetSource source)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var asset = new AssetContext(source);

            var gltf = source.GLTF;

            foreach(var texture in gltf.textures)
            {
                var image = gltf.images[texture.source];
                var bytes = source.GetImageBytes(image);
                asset._textureImages.Add(new ImageBytes(bytes));
            }

            foreach (var material in gltf.materials)
            {
                var shader = material.IsUnlit
                    ? ShaderLoader.Instance.CreateShader(ShaderType.Unlit)
                    : ShaderLoader.Instance.CreateShader(ShaderType.Standard)
                    ;

                var texture = default(ImageBytes);
                var color = Color4.White;
                var pbr = material.pbrMetallicRoughness;
                if (pbr != null)
                {
                    if (pbr.baseColorTexture != null)
                    {
                        texture = asset._textureImages[pbr.baseColorTexture.index];
                    }

                    if (pbr.baseColorFactor != null)
                    {
                        color.Red = pbr.baseColorFactor[0];
                        color.Green = pbr.baseColorFactor[1];
                        color.Blue = pbr.baseColorFactor[2];
                        color.Alpha = pbr.baseColorFactor[3];
                    }
                }
                asset._materials.Add(new D3D11Material(material.name, shader, true, texture, color));
            }

            foreach(var mesh in gltf.meshes)
            {
                asset._meshes.Add(Mesh.FromGLTF(source, mesh, asset._materials));
            }

            foreach(var skin in gltf.skins)
            {
                asset._skins.Add(Skin.FromGLTF(source, skin));
            }

            Logger.Info($"LoadAsset: {sw.Elapsed.TotalSeconds} sec");

            asset.BuildHierarchy();

            return asset;
        }

        void BuildHierarchy()
        {
            var gltf = _source.GLTF;

            _nodes = gltf.nodes.Select((x, i) => CreateDrawable(i, x)).ToArray();

            for (int i = 0; i < gltf.nodes.Count; ++i)
            {
                var node = gltf.nodes[i];
                var drawable = _nodes[i];

                if (node.children != null)
                {
                    // build hierarchy
                    foreach (var j in node.children)
                    {
                        drawable.AddChild(_nodes[j]);
                    }
                }

                if (node.mesh >= 0)
                {
                    drawable.Mesh = _meshes[node.mesh];

                    if (node.skin >= 0)
                    {
                        drawable.Mesh.SetSkin(_skins[node.skin], _nodes);
                    }
                }
            }
        }

        public static Node CreateDrawable(int i, UniGLTF.glTFNode node)
        {
            var name = $"[{i}]{node.name}";
            var drawable = new Node(name);

            if (node.matrix != null)
            {
                var m = new Matrix(node.matrix);
                drawable.LocalMatrix = m;
            }
            else
            {
                var t = Vector3.Zero;
                var r = Quaternion.Identity;
                var s = Vector3.One;

                if (node.translation != null)
                {
                    t.X = node.translation[0];
                    t.Y = node.translation[1];
                    t.Z = node.translation[2];
                }

                if (node.rotation != null)
                {
                    r.X = node.rotation[0];
                    r.Y = node.rotation[1];
                    r.Z = node.rotation[2];
                    r.W = node.rotation[3];
                }

                if (node.scale != null)
                {
                    s.X = node.scale[0];
                    s.Y = node.scale[1];
                    s.Z = node.scale[2];
                }

                var m = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, s, Vector3.Zero, r, t);
                drawable.LocalMatrix = m;
            }

            return drawable;
        }



    }
}
