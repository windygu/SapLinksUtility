﻿namespace FileIO
{
    internal interface IDataFile
    {
        /// <summary>
        /// Opens a text file for reading.
        /// </summary>
        /// <param name="filePath">The file path of the file to be opened
        /// </param>
        void OpenForRead(string filePath);

        /// <summary>
        /// Opens a text file for reading.
        /// </summary>
        /// <param name="directoryPath">The directory path where the file is
        /// located</param>
        /// <param name="fileName">The file name</param>
        void OpenForRead(string directoryPath, string fileName);

        /// <summary>
        /// Opens an existing text file for writing.
        /// </summary>
        /// <param name="filePath">The file path of the file to be opened
        /// </param>
        void OpenForWrite(string filePath);

        /// <summary>
        /// Opens an existing text file for writing.
        /// </summary>
        /// <param name="directoryPath">The directory path where the file is
        /// located</param>
        /// <param name="fileName">The file name</param>
        void OpenForWrite(string directoryPath, string fileName);

        /// <summary>
        /// Creates a new text file and opens it for writing.
        /// </summary>
        /// <param name="filePath">The file path of the file to be created and
        /// opened</param>
        void CreateForWrite(string filePath);

        /// <summary>
        /// Creates a new text file and opens it for writing.
        /// </summary>
        /// <param name="directoryPath">The directory path where the file is
        /// located</param>
        /// <param name="fileName">The file name</param>
        void CreateForWrite(string directoryPath, string fileName);

        /// <summary>
        /// Read a line from a text file and advance the file to the next line.
        /// </summary>
        /// <returns>Returns a line as a string object, or returns a null
        /// string if we are at the end of the file.</returns>
        string ReadLine();

        /// <summary>
        /// Write a line to the end of the text file.
        /// </summary>
        /// <param name="line">The line to be written to the end of the file
        /// </param>
        void WriteLine(string line);

        /// <summary>
        /// Close the text file
        /// </summary>
        void Close();

        /// <summary>
        /// Save the text file to disk
        /// </summary>
        void Save();

        /// <summary>
        /// Move the file a different directory
        /// </summary>
        /// <param name="directoryPath">Target directory path</param>
        void Move(string directoryPath);

        /// <summary>
        /// Copy the file to another directory
        /// </summary>
        /// <param name="directoryPath">Target directory</param>
        void Copy(string directoryPath);

        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="fileName">New file name</param>
        void Rename(string fileName);

        /// <summary>
        /// Return the name of the current file.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Return the directory path to the current file.
        /// </summary>
        string DirectoryPath { get; }

        /// <summary>
        /// Returns the full file path of the current file.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Returns the current file state (initial, opened, or closed).
        /// </summary>
        FileState State { get; }

        /// <summary>
        /// Returns the current file mode (initial, read, or write).
        /// </summary>
        FileMode Mode { get; }

        /// <summary>
        /// Returns the number of lines in the data file
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns a zero-based index to the current line in the file
        /// </summary>
        int Position { get; }
    }
}