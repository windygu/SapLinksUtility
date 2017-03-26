using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when issues are detected opening a file
    /// </summary>
    internal class FileOpenException : ApplicationException
    {
        private const string _fileOpenMessage = "File open exception";
        private string _msg = null;
        private string _filePath = null;
        private string _fileName = null;

        public FileOpenException() : base(_fileOpenMessage)
        {
        }

        public FileOpenException(string msg) :
            base(String.Format("{0}: {1}", _fileOpenMessage, msg))
        {
            _msg = msg;
        }

        public FileOpenException(string msg, Exception inner) :
            base(String.Format("{0}: {1}", _fileOpenMessage, msg), inner)
        {
            _msg = msg;
        }

        public override string Message
        {
            get
            {
                return _msg;
            }
        }

        public string FilePath
        {
            get
            {
                if (_filePath == null) return "NULL";
                else return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }

        public string FileName
        {
            get
            {
                if (_fileName == null) return "NULL";
                else return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }
    }
}