using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when errors occur coying, moving, or renaming a file
    /// </summary>
    public class FileOperationException : ApplicationException
    {
        private const string _fileMovementMessage = "File operation exception";
        private string _msg = null;
        private string _source = null;
        private string _target = null;

        public FileOperationException() : base(_fileMovementMessage)
        {
        }

        public FileOperationException(string msg) :
            base($"{_fileMovementMessage}: {msg}")
        {
            _msg = msg;
        }

        public FileOperationException(string msg, Exception inner) :
            base($"{_fileMovementMessage}: {msg}", inner)
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

        public string SourcePath
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        public string TargetPath
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
            }
        }
    }
}