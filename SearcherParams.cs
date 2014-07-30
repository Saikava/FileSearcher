using System;
using System.Text;
using System.Collections.Generic;



namespace FileSearcher
{
    public class SearcherParams
    {
        private readonly String m_searchDir;
        private readonly Boolean m_includeSubDirsChecked;
        private readonly List<String> m_fileNames;
        private readonly Boolean m_containingChecked;
        private readonly String m_containingText;
        private readonly Encoding m_encoding;

        public SearcherParams(  String searchDir,
                                List<String> fileNames,
                                String containingText,
                                Encoding encoding)
        {
            m_searchDir = searchDir;
            m_includeSubDirsChecked = true;
            m_fileNames = fileNames;
            m_containingChecked = true;
            m_containingText = containingText;
            m_encoding = encoding;
        }

        public String SearchDir
        {
            get { return m_searchDir; }
        }

        public Boolean IncludeSubDirsChecked
        {
            get { return m_includeSubDirsChecked; }
        }

        public List<String> FileNames
        {
            get { return m_fileNames; }
        }

        public Boolean ContainingChecked
        {
            get { return m_containingChecked; }
        }

        public String ContainingText
        {
            get { return m_containingText; }
        }

        public Encoding Encoding
        {
            get { return m_encoding; }
        }
    }
}
