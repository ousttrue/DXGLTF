using DXGLTF.Assets;
using GltfScene;
using NLog;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DXGLTF
{
    class SceneHierarchy : TreeViewContentBase
    {
        static Logger Logger = LogManager.GetCurrentClassLogger();

        ShaderLoader _shaderLoader = new ShaderLoader();

        Assets.AssetContext _asset;
        Assets.AssetContext Asset
        {
            get { return _asset; }
            set
            {
                var tmp = default(AssetContext);
                lock (this)
                {
                    tmp = _asset;
                    _asset = value;
                }

                if (tmp != null)
                {
                    tmp.Dispose();
                }
            }
        }

        public SceneHierarchy(Scene scene) : base(scene)
        { }

        protected override void OnUpdated(Source source)
        {
            LoadAsset(source);
        }

        async void LoadAsset(Source source)
        {
            if (source.GlTF == null)
            {
                Asset = null;
                return;
            }

            Asset = await Task.Run(() => AssetContext.Load(source, _shaderLoader));
        }

        protected override void OnSelected(TreeNode node)
        {
        }
    }
}
