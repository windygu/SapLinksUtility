using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when an encoded text file is found to be corrupted
    /// </summary>
    internal class CorruptedFileException : ApplicationException
    {
        private const string _fileSystemMessage = "Corrupted file exception";
        private string _msg = null;
        private string _filePath = null;
        private string _fileName = null;

        public CorruptedFileException() : base(_fileSystemMessage)
        {
        }

        public CorruptedFileException(string msg) :
            base(String.Format("{0}: {1}", _fileSystemMessage, msg))
        {
            _msg = msg;
        }

        public CorruptedFileException(string msg, Exception inner) :
            base(String.Format("{0}: {1}", _fileSystemMessage, msg), inner)
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