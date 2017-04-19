using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileIO;
using System.IO;

namespace FileIOTest
{
    /// <summary>
    /// Summary description for TextFileTests
    /// </summary>
    [TestClass]
    public class TextFileTests
    {
        private const string NULL = "NULL";
        private const string WriteFile_name = "textfile1.txt";
        private const string WriteFile_path = @"\" + WriteFile_name;
        private const string ReadFile_name = "testfile3.txt";
        private const string ReadFile_path = @"\" + ReadFile_name;
        private const string ClosedFile_name = "testfile4.txt";
        private const string ClosedFile_path = @"\" + ClosedFile_name;
        private static TextFile writeFile = new TextFile(); // used for testing "write" mode stuff
        private static TextFile initialFile = new TextFile(); // used for testing "initial" mode stuff
        private static TextFile readFile = new TextFile(); // used for testing "read" mode stuff
        private static TextFile closedFile = new TextFile(); // used for testing "closed" mode stuff
        private static string filePath = Directory.GetCurrentDirectory();

        [ClassInitialize]
        public static void TextFileSetup(TestContext tc)
        {
            writeFile.CreateForWrite(filePath + WriteFile_path);
            writeFile.WriteLine("The first line");
            writeFile.WriteLine("The second line");
            readFile.CreateForWrite(filePath + ReadFile_path);
            readFile.WriteLine("First line");
            readFile.WriteLine("Second line");
            readFile.WriteLine("Third Line");
            readFile.WriteLine("Fourth line");
            readFile.Close();
            readFile.OpenForRead(filePath + ReadFile_path);
            closedFile.CreateForWrite(filePath + ClosedFile_path);
            closedFile.WriteLine("This is a test");
            closedFile.Close();
        }

        [ClassCleanup]
        public static void TextFileCleanup()
        {
            writeFile.Close();
            File.Delete(filePath + WriteFile_path);
            initialFile.Close();
            readFile.Close();
            File.Delete(filePath + ReadFile_path);
            File.Delete(filePath + ClosedFile_path);
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Copy_fileNotOpen()
        {
            try
            {
                closedFile.Copy("target");
            }
            catch (FileOpenException e)
            {
                Directory.Delete("target");
                Assert.AreEqual("File not open", e.Message);
                return;
            }
            Directory.Delete("target");
            Assert.Fail("Copy_fileNotOpen failed to throw the expected exception");
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Copy_nullTargetPath()
        {
            try
            {
                readFile.Copy(null);
            }
            catch (FilePathException e)
            {
                Assert.AreEqual("Target path is missing", e.Message);
                return;
            }
            Assert.Fail("Copy_nullTargetPath failed to throw the expected exception");
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Copy_sourceSameAsTarget()
        {
            try
            {
                readFile.Copy(readFile.DirectoryPath);
            }
            catch (FileOperationException e)
            {
                Assert.AreEqual("Source and target are the same", e.Message);
                return;
            }
            Assert.Fail("Copy_sourceSameAsTarget failed to throw the expected exception");
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Copy_success()
        {
            string targetPath = "copy_path";
            Directory.CreateDirectory(targetPath);
            try
            {
                readFile.Copy(targetPath);
            }
            catch (Exception e)
            {
                Directory.Delete(targetPath);
                Assert.Fail($"Copy_success threw an unexpected exception\nMessage: {e.Message}");
            }
            if (!File.Exists(targetPath + @"\" + readFile.FileName))
            {
                Directory.Delete(targetPath);
                Assert.Fail("Copy_success failed to create the target file");
            }
            File.Delete(targetPath + @"\" + readFile.FileName);
            Directory.Delete(targetPath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Copy_targetAlreadyExists()
        {
            string testPath = Directory.GetCurrentDirectory() + @"\test";
            Directory.CreateDirectory(testPath);
            using (File.Create(testPath + ReadFile_path)) { }
            try
            {
                readFile.Copy(testPath);
            }
            catch (FileOpenException e)
            {
                File.Delete(testPath + ReadFile_path);
                Directory.Delete(testPath);
                Assert.AreEqual("Target file already exists", e.Message);
                return;
            }
            File.Delete(testPath + ReadFile_path);
            Directory.Delete(testPath);
            Assert.Fail("Copy_targetAlreadyExists failed to throw the expected exception");
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Count_validForClosed()
        {
            Assert.AreEqual(0, closedFile.Count);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Count_validForInitial()
        {
            Assert.AreEqual(0, initialFile.Count);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Count_validForRead()
        {
            Assert.AreEqual(4, readFile.Count);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Count_validForWrite()
        {
            Assert.AreEqual(2, writeFile.Count);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void DirectoryPath_nullClosed()
        {
            Assert.AreEqual(NULL, closedFile.DirectoryPath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void DirectoryPath_nullInitial()
        {
            Assert.AreEqual(NULL, initialFile.DirectoryPath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void DirectoryPath_validRead()
        {
            Assert.AreEqual(filePath, readFile.DirectoryPath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void DirectoryPath_validWrite()
        {
            Assert.AreEqual(filePath, writeFile.DirectoryPath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileMode_validClosed()
        {
            Assert.AreEqual(FileIO.FileMode.INITIAL, closedFile.Mode);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileMode_validInitial()
        {
            Assert.AreEqual(FileIO.FileMode.INITIAL, initialFile.Mode);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileMode_validRead()
        {
            Assert.AreEqual(FileIO.FileMode.READ, readFile.Mode);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileMode_validWrite()
        {
            Assert.AreEqual(FileIO.FileMode.WRITE, writeFile.Mode);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileName_nullClosed()
        {
            Assert.AreEqual(NULL, closedFile.FileName);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileName_nullInitial()
        {
            Assert.AreEqual(NULL, initialFile.FileName);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileName_validRead()
        {
            Assert.AreEqual(ReadFile_name, readFile.FileName);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileName_validWrite()
        {
            Assert.AreEqual(WriteFile_name, writeFile.FileName);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FilePath_nullClosed()
        {
            Assert.AreEqual(NULL, closedFile.FilePath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FilePath_nullInitial()
        {
            Assert.AreEqual(NULL, initialFile.FilePath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FilePath_validRead()
        {
            Assert.AreEqual(filePath + ReadFile_path, readFile.FilePath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FilePath_validWrite()
        {
            Assert.AreEqual(filePath + WriteFile_path, writeFile.FilePath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Position_validClosed()
        {
            Assert.AreEqual(-1, closedFile.Position);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Position_validInitial()
        {
            Assert.AreEqual(-1, initialFile.Position);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Position_validRead()
        {
            Assert.AreEqual(0, readFile.Position);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Position_validWrite()
        {
            Assert.AreEqual(2, writeFile.Position);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void State_validClosed()
        {
            Assert.AreEqual(FileState.CLOSED, closedFile.State);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void State_validInitial()
        {
            Assert.AreEqual(FileState.INITIAL, initialFile.State);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void State_validRead()
        {
            Assert.AreEqual(FileState.OPEN, readFile.State);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void State_validWrite()
        {
            Assert.AreEqual(FileState.OPEN, writeFile.State);
        }
    }
}
