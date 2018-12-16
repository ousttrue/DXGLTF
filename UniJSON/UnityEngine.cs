using UniJSON;

namespace UnityEngine
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }

    public static class JsonUtility
    {
        public static string ToJson<T>(T value)
        {
            var f = new JsonFormatter();
            f.Serialize(value);
            return new Utf8String(f.GetStoreBytes()).ToString();
        }

        public static T FromJson<T>(string json)
        {
            var parsed = JsonParser.Parse(json);
            var t = default(T);
            parsed.Deserialize(ref t);
            return t;
        }
    }
}
