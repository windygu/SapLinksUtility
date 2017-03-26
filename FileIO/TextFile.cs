using System;
using System.Collections.Generic;
using System.IO;

namespace FileIO
{
    /// <summary>
    /// The FileMode enumeration tells which mode a file has been opened in -
    /// INITIAL - The file hasn't been opened yet.
    /// READ - The file has been opened for reading.
    /// WRITE - The file has been opened for writing.
    /// </summary>
    public enum FileMode { INITIAL, READ, WRITE }

    /// <summary>
    /// The FileState enumeration indicates the state of the current file -
    /// INITIAL - The file object has been created but not opened yet.
    /// OPEN - Then file object has been opened.
    /// CLOSED - The file object has been closed.
    /// </summary>
    public enum FileState { INITIAL, OPEN, CLOSED }

    /// <summary>
    /// Object represents a plain text flat file. Implements IDataFile interface.
    /// </summary>
    public class TextFile : IDataFile
    {
        /// <summary>
        /// The name of the file to be read from or written to.
        /// </summary>
        private string _fileName = null;

        /// <summary>
        /// The absolute directory path where the file is located
        /// </summary>
        private string _directoryPath = null;

        /// <summary>
        /// The current state of the file (INITIAL, OPEN, or CLOSED)
        /// </summary>
        private FileState _fileState = FileState.INITIAL;

        /// <summary>
        /// The file mode (INITIAL, READ, or WRITE)
        /// </summary>
        private FileMode _fileMode = FileMode.INITIAL;

        /// <summary>
        /// A collection of strings. These are the lines that are either read
        /// from or written to the file.
        /// </summary>
        protected List<string> _fileData = new List<string>();

        /// <summary>
        /// For files opened for reading, this gives the index of the next line
        /// to be read from the _fileData collection. For files opened for
        /// writing, this gives the index of the next line to be written to the
        /// _fileData collection. A value of -1 means the file is empty.
        /// </summary>
        private int _filePosition = -1;

        /// <summary>
        /// The default constructor doesn't do anything other than to
        /// instantiate a TextFile object.
        /// </summary>
        public TextFile() { }

        /// <summary>
        /// This is a finalizer that closes the text file if it hasn't been
        /// closed already before handing things over to the garbage collector.
        /// </summary>
        ~TextFile()
        {
            if (State == FileState.OPEN) Close();
        }

        /// <summary>
        /// Public property that returns the file name of the TextFile object
        /// </summary>
        public virtual string FileName
        {
            get
            {
                return _fileName;
            }
            protected set
            {
                _fileName = value;
            }
        }

        /// <summary>
        /// Public property that returns the file path of the TextFile object
        /// </summary>
        public virtual string DirectoryPath
        {
            get
            {
                return _directoryPath;
            }
            protected set
            {
                _directoryPath = value;
            }
        }

        /// <summary>
        /// Public property that returns the full file path and file name of
        /// the TextFile object
        /// </summary>
        public virtual string FilePath
        {
            get
            {
                return Path.Combine(_directoryPath, _fileName);
            }
        }

        /// <summary>
        /// Protected property for getting and setting the file state
        /// </summary>
        public virtual FileState State
        {
            get
            {
                return _fileState;
            }
            protected set
            {
                _fileState = value;
            }
        }

        /// <summary>
        /// Protected property for getting and setting the file mode
        /// </summary>
        public virtual FileMode Mode
        {
            get
            {
                return _fileMode;
            }
            protected set
            {
                _fileMode = value;
            }
        }

        /// <summary>
        /// Public property that returns the number of lines in the file
        /// </summary>
        public virtual int Count
        {
            get
            {
                return _fileData.Count;
            }
        }

        /// <summary>
        /// Public property that returns a zero-based index to the current line
        /// in the file
        /// </summary>
        public virtual int Position
        {
            get
            {
                return _filePosition;
            }
            protected set
            {
                _filePosition = value;
            }
        }

        /// <summary>
        /// Open an existing text file for reading
        /// </summary>
        /// <param name="filePath">The file path and file name of the text file
        /// to be opened</param>
        public virtual void OpenForRead(string filePath)
        {
            // Check to see if the file is already open. Throw an exception if
            // it is.
            if (State == FileState.OPEN)
            {
                FileOpenException foe = new FileOpenException("File already open");
                foe.FilePath = DirectoryPath;
                foe.FileName = FileName;
                throw foe;
            }
            // Obtain the absolute file path. Throw an exception if the path is
            // invalid.
            string absoluteFilePath = ParseFilePath(filePath);
            // Throw an exception if the file doesn't exist.
            FileOps.FileMustExist(absoluteFilePath);
            State = FileState.OPEN;
            Mode = FileMode.READ;
            // Read the contents of the file into the _fileData collection
            try
            {
                using (StreamReader sr = new StreamReader(absoluteFilePath))
                {
                    while (sr.Peek() >= 0) // Check to see if we've reached the end of the file
                    {
                        _fileData.Add(sr.ReadLine()); // Read the next line from the file
                    }
                    if (Count > 0) Position = 0; // Set the file pointer to the first line
                    else Position = -1;
                }
            }
            catch (Exception e)
            {
                string msg = String.Format("Error reading line {0} from file", Count + 1);
                FileIOException fie = new FileIOException(msg, e);
                fie.FilePath = DirectoryPath;
                fie.FileName = FileName;
                throw fie;
            }
        }

        /// <summary>
        /// Open an existing text file for reading
        /// </summary>
        /// <param name="directoryPath">The directory path where the file is
        /// located</param>
        /// <param name="fileName">The file name of the text file to be opened
        /// </param>
        public virtual void OpenForRead(string directoryPath, string fileName)
        {
            string FilePath = FileOps.CombinePath(directoryPath, fileName);
            OpenForRead(FilePath);
        }

        /// <summary>
        /// Open an existing text file for writing
        /// </summary>
        /// <param name="filePath">The file path and file name of the text file
        /// to be opened</param>
        public virtual void OpenForWrite(string filePath)
        {
            // Check to see if the file is already open. Throw an exception if
            // it is.
            if (State == FileState.OPEN)
            {
                FileOpenException foe = new FileOpenException("File already open");
                foe.FilePath = DirectoryPath;
                foe.FileName = FileName;
                throw foe;
            }
            // Obtain the absolute file path.
            string absoluteFilePath = ParseFilePath(filePath);
            // Throw an exception if the file doesn't exist.
            FileOps.FileMustExist(absoluteFilePath);
            // Finish opening the text file for writing
            State = FileState.OPEN;
            Mode = FileMode.WRITE;
            Position = 0;
        }

        /// <summary>
        /// Open an existing text file for writing
        /// </summary>
        /// <param name="directoryPath">The directory path where the file is
        /// located</param>
        /// <param name="fileName">The file name of the text file to be opened
        /// </param>
        public virtual void OpenForWrite(string directoryPath, string fileName)
        {
            string filePath = FileOps.CombinePath(directoryPath, fileName);
            OpenForWrite(filePath);
        }

        /// <summary>
        /// Creates a new text file for writing
        /// </summary>
        /// <param name="filePath">The file path of the text file to be opened
        /// </param>
        public virtual void CreateForWrite(string filePath)
        {
            // Check to see if the file is already open. Throw an exception if
            // it is.
            if (State == FileState.OPEN)
            {
                FileOpenException foe = new FileOpenException("File already open");
                foe.FilePath = DirectoryPath;
                foe.FileName = FileName;
                throw foe;
            }
            // Obtain the absolute file path.
            string absoluteFilePath = ParseFilePath(filePath);
            // Throw an exception if the file already exists.
            FileOps.FileMustNotExist(absoluteFilePath);
            // Create the new text file
            FileOps.CreateFile(absoluteFilePath);
            // Finish opening the text file for writing
            State = FileState.OPEN;
            Mode = FileMode.WRITE;
            Position = 0;
        }

        /// <summary>
        /// Creates a new text file for writing
        /// </summary>
        /// <param name="directoryPath">The directory path where the file is
        /// located</param>
        /// <param name="fileName">The file name of the text file to be opened
        /// </param>
        public virtual void CreateForWrite(string directoryPath, string fileName)
        {
            string filePath = FileOps.CombinePath(directoryPath, fileName);
            CreateForWrite(filePath);
        }

        /// <summary>
        /// Return the next line from the text file
        /// </summary>
        /// <returns>Returns a text string, or null if the end of file is
        /// reached</returns>
        public virtual string ReadLine()
        {
            if (Position < 0 || Position >= Count) return null;
            else
            {
                string line = _fileData[Position];
                _filePosition++;
                return line;
            }
        }

        /// <summary>
        /// Write a line to the text file. The line is saved in the _fileData
        /// collection until the file is closed, at which point all lines are
        /// flushed to the file.
        /// </summary>
        /// <param name="line"></param>
        public virtual void WriteLine(string line)
        {
            _fileData.Add(line);
            _filePosition++;
        }

        /// <summary>
        /// Close the text file. If the file was opened for writing, then first
        /// write the lines of text to the physical file.
        /// </summary>
        public virtual void Close()
        {
            if (State == FileState.OPEN)
            {
                if (Mode == FileMode.WRITE && Count > 0) Save();
                _fileData.Clear();
                Position = -1;
                DirectoryPath = null;
                FileName = null;
                Mode = FileMode.INITIAL;
                State = FileState.CLOSED;
            }
        }

        /// <summary>
        /// Save the contents of the text file to disk
        /// </summary>
        public virtual void Save()
        {
            string filePath = FileOps.CombinePath(DirectoryPath, FileName);
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    foreach (string line in _fileData)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = "NULL";
                FileIOException fie = new FileIOException("Unable to write file to disk", e);
                fie.FilePath = DirectoryPath;
                fie.FileName = FileName;
                throw fie;
            }
        }

        /// <summary>
        /// Verify that the given directory path is valid. Create the directory
        /// if it doesn't already exist. Return the absolute directory path.
        /// </summary>
        /// <param name="directoryPath">The directory path to be validated
        /// </param>
        /// <returns>Returns the absolute directory path</returns>
        private string ValidateTargetPath(string directoryPath)
        {
            // The target path can't be null or zero length
            if (directoryPath == null || directoryPath.Length == 0)
            {
                FilePathException fpe = new FilePathException("Target path is missing");
                if (State == FileState.OPEN) fpe.FileName = FileName;
                else fpe.FileName = "n/a";
                if (directoryPath == null) fpe.FilePath = "NULL";
                else fpe.FilePath = "EMPTY";
                throw fpe;
            }
            // Verify that the target file path doesn't contain any invalid
            // characters
            if (!FileOps.ValidDirectoryPath(directoryPath))
            {
                FilePathException fpe = new FilePathException("Target file path contains invalid characters");
                fpe.FilePath = directoryPath;
                fpe.FileName = "n/a";
                throw fpe;
            }
            // Get the full target file path
            string targetPath;
            try
            {
                targetPath = FileOps.GetAbsoluteFilePath(directoryPath);
            }
            catch (Exception e)
            {
                FilePathException fpe = new FilePathException("Invalid target file path", e);
                if (State == FileState.OPEN) fpe.FileName = FileName;
                else fpe.FileName = "n/a";
                fpe.FilePath = directoryPath;
                throw fpe;
            }
            // Verify that the target directory exists. Create the directory if
            // it doesn't exist.
            if (!Directory.Exists(targetPath))
            {
                try
                {
                    Directory.CreateDirectory(targetPath);
                }
                catch (Exception e)
                {
                    FileOperationException foe = new FileOperationException("Unable to create target directory", e);
                    if (targetPath == null) foe.TargetPath = "NULL";
                    else foe.TargetPath = targetPath;
                    foe.SourcePath = "n/a";
                    throw foe;
                }
            }
            return targetPath;
        }

        /// <summary>
        /// Move the file to a different directory
        /// </summary>
        /// <param name="directoryPath">The target directory path</param>
        public virtual void Move(string directoryPath)
        {
            // Get the absolute directory path
            string targetPath = ValidateTargetPath(directoryPath);
            // Verify that the file is open
            if (State != FileState.OPEN)
            {
                FileOpenException foe = new FileOpenException("File not open");
                foe.FileName = FileName;
                foe.FilePath = directoryPath;
                throw foe;
            }
            // Create the full target file path
            string fullFilePath;
            try
            {
                fullFilePath = Path.Combine(targetPath, FileName);
            }
            catch (Exception e)
            {
                FilePathException fpe = new FilePathException("Unable to determine full target file path", e);
                if (targetPath == null) fpe.FilePath = "NULL";
                else fpe.FilePath = targetPath;
                fpe.FileName = FileName;
                throw fpe;
            }
            // If the target path matches the source path, return without doing
            // anything
            if (fullFilePath == FilePath) return;
            // The target file must not already exist
            try
            {
                FileOps.FileMustNotExist(fullFilePath);
            }
            catch (Exception e)
            {
                FileOpenException foe = new FileOpenException("Target file already exists", e);
                foe.FileName = FileName;
                foe.FilePath = targetPath;
                throw foe;
            }
            // First copy the file to the target path
            try
            {
                File.Copy(FilePath, fullFilePath);
            }
            catch (Exception e)
            {
                FileOperationException foe = new FileOperationException("Error moving file to target location", e);
                foe.SourcePath = FilePath;
                foe.TargetPath = fullFilePath;
                throw foe;
            }
            // Delete the file from the source location if the copy operation
            // was successful
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception e)
            {
                FileOperationException foe = new FileOperationException("Error removing file from source location", e);
                foe.SourcePath = FilePath;
                foe.TargetPath = targetPath;
                throw foe;
            }
            DirectoryPath = targetPath;
        }

        /// <summary>
        /// Copy the file to a different directory
        /// </summary>
        /// <param name="directoryPath">The target directory path</param>
        public virtual void Copy(string directoryPath)
        {
            // Get the absolute directory path
            string targetPath = ValidateTargetPath(directoryPath);
            // Verify that the file is open
            if (State != FileState.OPEN)
            {
                FileOpenException foe = new FileOpenException("File not open");
                foe.FileName = FileName;
                foe.FilePath = DirectoryPath;
                throw foe;
            }
            // Create the full target file path
            string fullFilePath;
            try
            {
                fullFilePath = Path.Combine(targetPath, FileName);
            }
            catch (Exception e)
            {
                FilePathException fpe = new FilePathException("Unable to determine full target file path", e);
                if (targetPath == null) fpe.FilePath = "NULL";
                else fpe.FilePath = targetPath;
                fpe.FileName = FileName;
                throw fpe;
            }
            // The target file path must not match the source file path
            if (fullFilePath == FilePath)
            {
                FileOperationException foe = new FileOperationException("Source and target are the same");
                foe.SourcePath = FilePath;
                foe.TargetPath = fullFilePath;
                throw foe;
            }
            // The target file must not already exist
            try
            {
                FileOps.FileMustNotExist(fullFilePath);
            }
            catch (Exception e)
            {
                FileOpenException foe = new FileOpenException("Target file already exists", e);
                foe.FileName = FileName;
                foe.FilePath = targetPath;
                throw foe;
            }
            // Copy the file to the target path
            try
            {
                File.Copy(FilePath, fullFilePath);
            }
            catch (Exception e)
            {
                FileOperationException foe = new FileOperationException("Error moving file to target location", e);
                foe.SourcePath = FilePath;
                foe.TargetPath = fullFilePath;
                throw foe;
            }
        }

        /// <summary>
        /// Rename the file
        /// </summary>
        /// <param name="fileName">New file name</param>
        public virtual void Rename(string fileName)
        {
            // Verify that the target file name is not empty or null
            if (fileName == null || fileName.Length == 0)
            {
                FilePathException fpe = new FilePathException("Missing file name");
                fpe.FilePath = DirectoryPath;
                if (fileName == null) fpe.FileName = "NULL";
                else fpe.FileName = "EMPTY";
                throw fpe;
            }
            // Verify that the target file name is valid
            if (fileName.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                !FileOps.ValidFileName(fileName))
            {
                FilePathException fpe = new FilePathException("Target file name contains invalid characters");
                fpe.FilePath = DirectoryPath;
                fpe.FileName = fileName;
                throw fpe;
            }
            // Verify that the target file name doesn't already exist
            string targetPath = Path.Combine(DirectoryPath, fileName);
            try
            {
                FileOps.FileMustNotExist(targetPath);
            }
            catch (Exception e)
            {
                FileOperationException foe = new FileOperationException("Target file already exists", e);
                foe.SourcePath = FilePath;
                foe.TargetPath = targetPath;
                throw foe;
            }
            // Rename the file
            try
            {
                File.Move(FilePath, targetPath);
            }
            catch (Exception e)
            {
                FileOperationException foe = new FileOperationException("Unable to rename file", e);
                foe.SourcePath = FilePath;
                foe.TargetPath = targetPath;
                throw foe;
            }
            FileName = fileName;
        }

        /// <summary>
        /// Parse the given file path string and extract the directory path and
        /// file name.
        /// </summary>
        /// <param name="filePath">The file path string to be parsed</param>
        /// <returns>Returns the absolute file path as a string object
        /// </returns>
        protected string ParseFilePath(string filePath)
        {
            // Obtain the absolute file path. Throw an exception if the path is
            // invalid.
            string absoluteFilePath = FileOps.GetAbsoluteFilePath(filePath);
            // Parse the file path and extract the directory path
            DirectoryPath = FileOps.GetDirectoryPath(absoluteFilePath);
            // Throw an exception if the directory path is null
            if (DirectoryPath == null)
            {
                FilePathException fpe = new FilePathException("Directory path can't be null");
                fpe.FilePath = "NULL";
                if (FileName == null) fpe.FileName = "NULL";
                else fpe.FileName = FileName;
                throw fpe;
            }
            // Parse the file path and extract the file name
            FileName = FileOps.GetFileName(absoluteFilePath);
            // Throw an exception if the file name is null
            if (FileName == null)
            {
                FilePathException fpe = new FilePathException("File name can't be null");
                fpe.FilePath = DirectoryPath;
                fpe.FileName = "NULL";
                throw fpe;
            }
            return absoluteFilePath;
        }

        /// <summary>
        /// ToString displays the characteristics of the text file
        /// </summary>
        /// <returns>Returns a string containing information about the file
        /// </returns>
        public override string ToString()
        {
            string directoryPath = DirectoryPath;
            if (directoryPath == null) directoryPath = "NULL";
            string fileName = FileName;
            if (fileName == null) fileName = "NULL";
            string fileState = State.ToString();
            string fileMode = Mode.ToString();
            return String.Format(
                "Plain text file =========/n" +
                "File path: {0}\n" +
                "File name: {1}\n" +
                "File state: {2}\n" +
                "File mode: {3}\n" +
                "Number of lines: {4}" +
                "Current line: {5}",
                directoryPath, fileName, fileState, fileMode, Count, Position);
        }
    }
}