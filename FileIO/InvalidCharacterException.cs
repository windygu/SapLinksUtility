using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when plain text character value is larger then 0x00ff
    /// </summary>
    internal class InvalidCharacterException : ApplicationException
    {
        private const string _fileSystemMessage = "Invalid character exception";
        private string _msg = null;
        private string _filePath = null;
        private string _fileName = null;

        public InvalidCharacterException() : base(_fileSystemMessage)
        {
        }

        public InvalidCharacterException(string msg) :
            base(String.Format("{0}: {1}", _fileSystemMessage, msg))
        {
            _msg = msg;
        }

        public InvalidCharacterException(string msg, Exception inner) :
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