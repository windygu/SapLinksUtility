using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown for various file path issues
    /// </summary>
    internal class FilePathException : ApplicationException
    {
        private const string _filePathMessage = "File path exception";
        private string _msg = null;
        private string _filePath = null;
        private string _fileName = null;

        public FilePathException() : base(_filePathMessage)
        {
        }

        public FilePathException(string msg) :
            base(String.Format("{0}: {1}", _filePathMessage, msg))
        {
            _msg = msg;
        }

        public FilePathException(string msg, Exception inner) :
            base(String.Format("{0}: {1}", _filePathMessage, msg), inner)
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