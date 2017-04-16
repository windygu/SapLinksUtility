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
        private const string TextFile1_name = "textfile1.txt";
        private const string TextFile1_path = @"\" + TextFile1_name;
        private static TextFile tf1 = new TextFile();
        private static TextFile tf2 = new TextFile();
        private static string filePath = Directory.GetCurrentDirectory();

        [ClassInitialize]
        public static void TextFileSetup(TestContext tc)
        {
            tf1.CreateForWrite(filePath + TextFile1_path);
            tf1.WriteLine("The first line");
            tf1.WriteLine("The second line");
        }

        [ClassCleanup]
        public static void TextFileCleanup()
        {
            tf1.Close();
            File.Delete(filePath + TextFile1_path);
            tf2.Close();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("TextFile")]
        public void Count_valid()
        {
            Assert.AreEqual(2, tf1.Count);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void DirectoryPath_null()
        {
            Assert.AreEqual(NULL, tf2.DirectoryPath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void DirectoryPath_valid()
        {
            Assert.AreEqual(filePath, tf1.DirectoryPath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileMode_validInitial()
        {
            Assert.AreEqual(FileIO.FileMode.INITIAL, tf2.Mode);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileMode_validWrite()
        {
            Assert.AreEqual(FileIO.FileMode.WRITE, tf1.Mode);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileName_null()
        {
            Assert.AreEqual(NULL, tf2.FileName);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FileName_valid()
        {
            Assert.AreEqual(TextFile1_name, tf1.FileName);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FilePath_null()
        {
            Assert.AreEqual(NULL, tf2.FilePath);
        }

        [TestMethod]
        [TestCategory("TextFile")]
        public void FilePath_valid()
        {
            Assert.AreEqual(filePath + TextFile1_path, tf1.FilePath);
        }
    }
}
