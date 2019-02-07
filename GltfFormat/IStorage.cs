using System;
using System.IO;


namespace UniGLTF
{
    public interface IStorage
    {
        ArraySegment<Byte> Get(string url);

        /// <summary>
        /// Get original filepath if exists
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string GetPath(string url);
    }

    public class SimpleStorage : IStorage
    {
        ArraySegment<Byte> m_bytes;

        public SimpleStorage():this(new ArraySegment<byte>())
        {
        }

        public SimpleStorage(ArraySegment<Byte> bytes)
        {
            m_bytes = bytes;
        }

        public ArraySegment<byte> Get(string url)
        {
            return m_bytes;
        }

        public string GetPath(string url)
        {
            return null;
        }
    }

    public class FileSystemStorage : IStorage
    {
        string m_root;

        public FileSystemStorage(string root)
        {
            m_root = Path.GetFullPath(root);
        }

        class Cache
        {
            public string Path;
            public Byte[] Bytes;
        }
        Cache _cache;

        public ArraySegment<byte> Get(string url)
        {
            if (url.StartsWith("data:"))
            {
                return new ArraySegment<byte>(UriByteBuffer.ReadEmbeded(url));
            }

            var path = Path.Combine(m_root, url);
            if(_cache==null || _cache.Path != path)
            {
                _cache = new Cache
                {
                    Path = path,
                    Bytes = File.ReadAllBytes(path)
                };
            }
            return new ArraySegment<byte>(_cache.Bytes);
        }

        public string GetPath(string url)
        {
            if (url.StartsWith("data:"))
            {
                return null;
            }
            else
            {
                return Path.Combine(m_root, url).Replace("\\", "/");
            }
        }
    }
}
