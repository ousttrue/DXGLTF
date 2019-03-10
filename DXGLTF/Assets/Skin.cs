using SharpDX;


namespace DXGLTF.Assets
{
    public class Skin
    {
        public int RootIndex;
        Matrix[] _bindMatrices;
        int[] _joints;

        public void Update(Node[] nodes)
        {
            if (_matrices == null)
            {
                _matrices = new Matrix[_joints.Length];
            }

            var root = nodes[RootIndex];
            var rootMatrix = root.WorldMatrix;
            rootMatrix.Invert();

            for (int i = 0; i < _joints.Length; ++i)
            {
                _matrices[i] = _bindMatrices[i] * nodes[_joints[i]].WorldMatrix /** rootMatrix*/;
            }
        }

        Matrix[] _matrices;
        public Matrix[] Matrices
        {
            get { return _matrices; }
        }

        public static Skin FromGLTF(AssetSource source, UniGLTF.glTFSkin skin)
        {
            return new Skin
            {
                RootIndex = skin.skeleton,
                _bindMatrices = source.GLTF.GetArrayFromAccessor<Matrix>(source.IO, skin.inverseBindMatrices),
                _joints = skin.joints,
            };
        }
    }
}
