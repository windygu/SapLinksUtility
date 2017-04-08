using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown for file read/write errors
    /// </summary>
    public class FileIOException : ApplicationException
    {
        private const string _fileSystemMessage = "File I/O exception";
        private string _msg = null;
        private string _filePath = null;
        private string _fileName = null;

        public FileIOException() : base(_fileSystemMessage)
        {
        }

        public FileIOException(string msg) :
            base($"{_fileSystemMessage}: {msg}")
        {
            _msg = msg;
        }

        public FileIOException(string msg, Exception inner) :
            base($"{_fileSystemMessage}: {msg}", inner)
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