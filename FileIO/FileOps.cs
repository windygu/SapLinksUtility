using System;
using System.IO;

namespace FileIO
{
    /// <summary>
    /// Static class containing several basic file-based methods
    /// </summary>
    public static class FileOps
    {
        /// <summary>
        /// Combines to parts of a file path into one file path
        /// </summary>
        /// <param name="filePath1">The first portion of the file path</param>
        /// <param name="filePath2">The second portion of the file path</param>
        /// <returns>Returns the combined file path as a string object
        /// </returns>
        public static string CombinePath(string filePath1, string filePath2)
        {
            string combinedPath = null;
            try
            {
                combinedPath = Path.Combine(filePath1, filePath2);
            }
            catch (Exception e)
            {
                if (filePath1 == null) filePath1 = "NULL";
                if (filePath2 == null) filePath2 = "NULL";
                FilePathException fpe = new FilePathException("Unable to construct full file path", e);
                fpe.FilePath = filePath1;
                fpe.FileName = filePath2;
                throw fpe;
            }
            return combinedPath;
        }

        /// <summary>
        /// Returns the absolute file path for the given path string
        /// </summary>
        /// <param name="filePath">File path string</param>
        /// <returns>Returns the absolute file path as a string object</returns>
        public static string GetAbsoluteFilePath(string filePath)
        {
            string absoluteFilePath = null;
            try
            {
                absoluteFilePath = Path.GetFullPath(filePath);
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = "NULL";
                FilePathException fpe = new FilePathException("Unable to parse file path", e);
                fpe.FilePath = filePath;
                fpe.FileName = "n/a";
                throw fpe;
            }
            return absoluteFilePath;
        }

        /// <summary>
        /// Check to verify that the specified file exists. Throw an exception
        /// if it doesn't.
        /// </summary>
        /// <param name="filePath">The path to the file to be checked</param>
        public static void FileMustExist(string filePath)
        {
            if (!File.Exists(filePath))
            {
                if (filePath == null) filePath = "NULL";
                FileOpenException foe = new FileOpenException("File doesn't exist");
                foe.FilePath = filePath;
                foe.FileName = "n/a";
                throw foe;
            }
        }

        /// <summary>
        /// Check to verify that the specified file doesn't exist. Throw an
        /// exception if it does.
        /// </summary>
        /// <param name="filePath">The path to the file to be checked</param>
        public static void FileMustNotExist(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileOpenException foe = new FileOpenException("File already exists");
                foe.FilePath = filePath;
                foe.FileName = "n/a";
                throw foe;
            }
        }

        /// <summary>
        /// Parse the given file path string and locate the last directory
        /// separator character
        /// </summary>
        /// <param name="filePath">File path string to be parsed (directory
        /// path plus file name)</param>
        /// <returns>Returns an integer representing the location of the last
        /// directory separator character</returns>
        private static int FindEndOfDirectoryPath(string filePath)
        {
            int start = -1;
            int index = 0;
            while (start < filePath.Length && index > -1)
            {
                index = filePath.IndexOf(Path.DirectorySeparatorChar, start);
                if (index == -1) break;
                start = index + 1;
            }
            if (start < 1 || start >= filePath.Length) return -1;
            else return start - 1;
        }

        /// <summary>
        /// Return the directory path portion of a file path string (excluding
        /// the final directory separator character)
        /// </summary>
        /// <param name="filePath">The file path (directory path plus file
        /// name)</param>
        /// <returns>Returns the directory path as a string object</returns>
        public static string GetDirectoryPath(string filePath)
        {
            int i = FindEndOfDirectoryPath(filePath);
            if (i < 0) return null;
            else return filePath.Substring(0, i);
        }

        /// <summary>
        /// Return the file name portion of a file path string
        /// </summary>
        /// <param name="filePath">The file path (directory path plus file
        /// name)</param>
        /// <returns>Returns the file name as a string object</returns>
        public static string GetFileName(string filePath)
        {
            int i = FindEndOfDirectoryPath(filePath);
            if (i >= filePath.Length - 1) return null;
            else return filePath.Substring(i + 1);
        }

        /// <summary>
        /// Create a new file if it doesn't already exist
        /// </summary>
        /// <param name="filePath">The file path of the file to be created
        /// </param>
        public static void CreateFile(string filePath)
        {
            try
            {
                using (FileStream fs = File.Create(filePath)) { };
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = "NULL";
                FileOperationException foe = new FileOperationException("Unable to create file", e);
                foe.TargetPath = filePath;
                foe.SourcePath = "n/a";
                throw foe;
            }
        }

        /// <summary>
        /// Truncates an existing text file to remove all lines
        /// </summary>
        /// <param name="filePath">File path of the file to be truncated
        /// </param>
        public static void TruncateFile(string filePath)
        {
            try
            {
                using (FileStream fs = File.OpenWrite(filePath)) { };
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = "NULL";
                FileOpenException foe = new FileOpenException("Unable to truncate file", e);
                foe.FilePath = filePath;
                foe.FileName = "n/a";
                throw foe;
            }
        }

        /// <summary>
        /// Create the specified directory path unless it already exists
        /// </summary>
        /// <param name="directoryPath">The directory path to be created
        /// </param>
        public static void CreateDirectory(string directoryPath)
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception e)
            {
                if (directoryPath == null) directoryPath = "NULL";
                FileOperationException foe = new FileOperationException("Unable to create directory", e);
                foe.TargetPath = directoryPath;
                foe.SourcePath = "n/a";
                throw foe;
            }
        }

        /// <summary>
        /// Verify that the given directory path doesn't contain any invalid
        /// characters
        /// </summary>
        /// <param name="directoryPath">The directory path to be verified
        /// </param>
        /// <returns>Returns "true" if the directory path is valid.
        /// Otherwise returns "false".</returns>
        public static bool ValidDirectoryPath(string directoryPath)
        {
            foreach (char x in Path.GetInvalidPathChars())
            {
                if (directoryPath.IndexOf(x) >= 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Verify that the given file name doesn't contain any invalid
        /// characters
        /// </summary>
        /// <param name="fileName">The file name to be verified</param>
        /// <returns>Returns "true" if the file name is valid.
        /// Otherwise returns "false".</returns>
        public static bool ValidFileName(string fileName)
        {
            foreach (char x in Path.GetInvalidFileNameChars())
            {
                if (fileName.IndexOf(x) >= 0) return false;
            }
            return true;
        }
    }
}