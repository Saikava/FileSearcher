using System.IO;



namespace FileSearcher
{
    public class FoundInfoEventArgs
    {
        private readonly FileSystemInfo m_info;

        public FoundInfoEventArgs(FileSystemInfo info)
        {
            m_info = info;
        }

        public FileSystemInfo Info
        {
            get { return m_info; }
        }
    }
}
