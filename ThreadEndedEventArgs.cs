using System;


namespace FileSearcher
{
    public class ThreadEndedEventArgs
    {
        private readonly Boolean m_success;
        private readonly String m_errorMsg;

        public ThreadEndedEventArgs(Boolean success,
                                    String errorMsg)
        {
            m_success = success;
            m_errorMsg = errorMsg;
        }

        public Boolean Success
        {
            get { return m_success; }
        }

        public String ErrorMsg
        {
            get { return m_errorMsg; }
        }
    }
}
