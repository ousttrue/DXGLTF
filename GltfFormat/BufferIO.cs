using System;
using System.IO;


namespace UniGLTF
{
    public interface IBufferIO
    {
        ArraySegment<Byte> GetBytes(string uri);
    }

    public class FolderIO: IBufferIO
    {
        string m_baseFolder;
        public FolderIO(string baseFolder)
        {
            m_baseFolder = baseFolder;
        }

        public static FolderIO FromFile(string path)
        {
            return new FolderIO(Path.GetDirectoryName(path));
        }

        string m_cachePath;
        byte[] m_cache;
        public ArraySegment<Byte> GetBytes(string uri)
        {
            var path = Path.Combine(m_baseFolder, uri);
            if (path == m_cachePath && m_cache != null)
            {
                // use cache
            }
            else
            {
                m_cachePath = path;
                m_cache = File.ReadAllBytes(path);
            }
            return new ArraySegment<byte>(m_cache);
        }
    }
}
