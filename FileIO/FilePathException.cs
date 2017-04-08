using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown for various file path issues
    /// </summary>
    public class FilePathException : ApplicationException
    {
        private const string _filePathMessage = "File path exception";
        private string _msg = null;
        private string _filePath = null;
        private string _fileName = null;

        public FilePathException() : base(_filePathMessage)
        {
        }

        public FilePathException(string msg) :
            base($"{_filePathMessage}: {msg}")
        {
            _msg = msg;
        }

        public FilePathException(string msg, Exception inner) :
            base($"{_filePathMessage}: {msg}", inner)
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