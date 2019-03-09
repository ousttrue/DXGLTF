using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;


namespace DXGLTF.Assets
{
    public struct AssetSource
    {
        public string Path;
        public IStorage IO;
        public glTF GLTF;
        public ListTreeNode<JsonValue> JSON;

        public ArraySegment<byte> GetImageBytes(glTFImage image)
        {
            if (string.IsNullOrEmpty(image.uri))
            {
                return GLTF.GetViewBytes(IO, image.bufferView);
            }
            else
            {
                return IO.Get(image.uri);
            }
        }

        bool HasSameBuffer(glTFPrimitives lhs, glTFPrimitives rhs)
        {
            {
                var l = GLTF.accessors[lhs.indices];
                var r = GLTF.accessors[rhs.indices];
                if (l.componentType != r.componentType)
                {
                    return false;
                }
                if (l.type != r.type)
                {
                    return false;
                }
                if (l.bufferView != r.bufferView)
                {
                    return false;
                }
            }

            if (lhs.attributes.POSITION != rhs.attributes.POSITION) return false;
            if (lhs.attributes.NORMAL != rhs.attributes.NORMAL) return false;
            if (lhs.attributes.TEXCOORD_0 != rhs.attributes.TEXCOORD_0) return false;
            if (lhs.attributes.TANGENT != rhs.attributes.TANGENT) return false;
            if (lhs.attributes.COLOR_0 != rhs.attributes.COLOR_0) return false;
            if (lhs.attributes.JOINTS_0 != rhs.attributes.JOINTS_0) return false;
            if (lhs.attributes.WEIGHTS_0 != rhs.attributes.WEIGHTS_0) return false;

            return true;
        }

        public bool HasSameBuffer(IEnumerable<glTFPrimitives> primitives)
        {
            var it = primitives.GetEnumerator();
            if (!it.MoveNext())
            {
                return false;
            }

            int i = 1;
            var first = it.Current;

            while (it.MoveNext())
            {
                ++i;
                if (!HasSameBuffer(first, it.Current))
                {
                    return false;
                }
            }

            return i > 1;
        }
    }
}
