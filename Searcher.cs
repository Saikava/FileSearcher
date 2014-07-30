using System;
using System.Threading;
using System.IO;



namespace FileSearcher
{
    public class Searcher
    {
        public delegate void FoundInfoEventHandler(FoundInfoEventArgs e);
        public static event FoundInfoEventHandler FoundInfo;

        public delegate void ThreadEndedEventHandler(ThreadEndedEventArgs e);
        public static event ThreadEndedEventHandler ThreadEnded;

        public delegate void StringSearchEndedHandler(EventArgs e);
        public static event StringSearchEndedHandler StringSearchEnded;

        private static Thread m_thread ;
        private static Boolean m_stop;
        private static SearcherParams m_pars;
        private static Byte[] m_containingBytes;

        public static Boolean Start(SearcherParams pars)
        {
            var success = false;

            if (m_thread == null)
            {
                ResetVariables();

                m_pars = pars;

                m_thread = new Thread(SearchThread);
                m_thread.Start();

                success = true;
            }

            return success;
        }

        public static void Stop()
        {
            m_stop = true;
        }

        private static void ResetVariables()
        {
            m_thread = null;
            m_stop = false;
            m_pars = null;
            m_containingBytes = null;
        }

        private static void SearchThread()
        {
            var success = true;
            var errorMsg = "";

            // Поиск информации о файле, соответсвующей параметрам
            if ((m_pars.SearchDir.Length >= 3) && (Directory.Exists(m_pars.SearchDir)))
            {
                if (m_pars.FileNames.Count > 0)
                {
                    if (m_pars.ContainingChecked)
                    {
                        if (m_pars.ContainingText != "")
                        {
                            try
                            {
                                m_containingBytes = m_pars.Encoding.GetBytes(m_pars.ContainingText);
                            }
                            catch (Exception)
                            {
                                success = false;
                                errorMsg = "The string\r\n" + m_pars.ContainingText + "\r\ncannot be converted into bytes.";
                            }
                        }
                        else
                        {
                            success = false;
                            errorMsg = "The string to search for must not be empty.";
                        }
                    }

                    if (success)
                    {
                        // Получение информации о папке
                        DirectoryInfo dirInfo = null;
                        try
                        {
                            dirInfo = new DirectoryInfo(m_pars.SearchDir);
                        }
                        catch (Exception ex)
                        {
                            success = false;
                            errorMsg = ex.Message;
                        }

                        if (success)
                        {
                            if (FoundInfo != null)
                            {
                                FoundInfo(new FoundInfoEventArgs(dirInfo));
                            }
                            SearchDirectory(dirInfo);

                        }
                    }
                }
                else
                {
                    success = false;
                    errorMsg = "Please enter one or more filenames to search for.";
                }
            }
            else
            {
                success = false;
                errorMsg = "The directory\r\n" + m_pars.SearchDir + "\r\ndoes not exist.";
            }

            m_thread = null;

            if (ThreadEnded != null)
            {
                ThreadEnded(new ThreadEndedEventArgs(success, errorMsg));
            }
        }

        private static void SearchDirectory(DirectoryInfo dirInfo)
        {
            if (!m_stop)
            {
                try
                {
                    foreach (var fileName in m_pars.FileNames)
                    {
                        var infos = dirInfo.GetFileSystemInfos(fileName);

                        foreach (var info in infos)
                        {
                            if (m_stop)
                            {
                                break;
                            }

                            if (MatchesRestrictions(info))
                            {
                                if (FoundInfo != null)
                                {
                                    FoundInfo(new FoundInfoEventArgs(info));
                                }
                            }
                        }
                    }

                    if (m_pars.IncludeSubDirsChecked)
                    {
                        var subDirInfos = dirInfo.GetDirectories();
                        foreach (var subDirInfo in subDirInfos)
                        {
                            if (m_stop)
                            {
                                break;
                            }

                            SearchDirectory(subDirInfo);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static Boolean MatchesRestrictions(FileSystemInfo info)
        {
            var matches = true;

            if (m_pars.ContainingChecked)
            {
                matches = false;
                if (info is FileInfo)
                {
                    matches = FileContainsBytes(info.FullName, m_containingBytes);
                }
            }

            if (StringSearchEnded != null)
            {
                StringSearchEnded(new EventArgs());
            }

            return matches;
        }

        private static Boolean FileContainsBytes(String path, Byte[] compare)
        {
            var contains = false;

            const int blockSize = 4096;
            if ((compare.Length >= 1) && (compare.Length <= blockSize))
            {
                var block = new Byte[compare.Length - 1 + blockSize];

                try
                {
                    var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                    // Read the first bytes from the file into "block":
                    var bytesRead = fs.Read(block, 0, block.Length);

                    do
                    {
                        // Search "block" for the sequence "compare":
                        var endPos = bytesRead - compare.Length + 1;
                        for (var i = 0; i < endPos; i++)
                        {
                            // Read "compare.Length" bytes at position "i" from the buffer,
                            // and compare them with "compare":
                            Int32 j;
                            for (j = 0; j < compare.Length; j++)
                            {
                                if (block[i + j] != compare[j])
                                {
                                    break;
                                }
                            }

                            if (j == compare.Length)
                            {
                                // "block" contains the sequence "compare":
                                contains = true;
                                break;
                            }
                        }

                        // Search completed?
                        if (contains || (fs.Position >= fs.Length))
                        {
                            break;
                        }
                        // Copy the last "compare.Length - 1" bytes to the beginning of "block":
                        for (var i = 0; i < (compare.Length - 1); i++)
                        {
                            block[i] = block[blockSize + i];
                        }

                        // Read the next "blockSize" bytes into "block":
                        bytesRead = compare.Length - 1 + fs.Read(block, compare.Length - 1, blockSize);
                    }
                    while (!m_stop);

                    fs.Close();
                }
                catch (Exception)
                {
                }
            }

            return contains;
        }
    }
}