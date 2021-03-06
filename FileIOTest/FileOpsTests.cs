﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace FileIO.Tests
{
    [TestClass]
    public class FileOpsTests
    {
        #region Private Fields

        private readonly string[,] combinePath_InvalidPath_data =
        {
            { null, @"\\dir)2" },
            { @"dir""1", @"dir2" },
            { "dir(1", null },
            { "dir1", "dir\"2" },
            { "dir<1", "dir2" }
        };

        private readonly string[,] combinePath_ValidPath_data =
        {
            { @"\dir(1\dir)2", @"dir3\testfile.txt", @"\dir(1\dir)2\dir3\testfile.txt" },
            { @"\dir1\dir2\", @"dir3\$testfile.txt", @"\dir1\dir2\dir3\$testfile.txt" },
            { @"\dir1\dir2", @"dir3", @"\dir1\dir2\dir3" },
            { @"\dir1\dir2\", @"dir3", @"\dir1\dir2\dir3" },
            { @"\dir1\dir2", @"dir3\", @"\dir1\dir2\dir3\" },
            { @"\dir1\dir2\", @"dir3\", @"\dir1\dir2\dir3\" },
            { @"dir1\dir2", @"dir3\testfile.txt", @"dir1\dir2\dir3\testfile.txt" },
            { @"dir1\dir-2\", @"dir3\testfile.txt", @"dir1\dir-2\dir3\testfile.txt" },
            { @"dir1\dir2", @"dir3", @"dir1\dir2\dir3" },
            { @"dir1\dir2\", @"dir3", @"dir1\dir2\dir3" },
            { @"dir+1\dir2", @"dir3\", @"dir+1\dir2\dir3\" },
            { @"dir1\dir2\", @"dir3\", @"dir1\dir2\dir3\" },
            { @"dir1", @"dir2", @"dir1\dir2" },
            { @"\dir1", @"\dir2", @"\dir2" },
            { @"dir1", @"\\dir2", @"\\dir2" }
        };

        private readonly int[] findEndOfDirectoryPath_expectedData =
        {
            12, 0, 5, 4, -1, -1, -1
        };

        private readonly string[] findEndOfDirectoryPath_pathData =
        {
            @"C:\This\is\a\test",
            @"\test",
            @"\test\test",
            @"test\",
            @"test",
            null,
            ""
        };

        private readonly string[] getAbsoluteFilePath_InvalidPath_data =
        {
            @"this\is\a\te:st",
            null,
            "",
            "   ",
            @"th""is\is\a\test",
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb" +
                "ccccccccccccccccccccccccccccccccccccccccccccccccccdddddddddddddddddddddddddddddddddddddddddddddd" +
                "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"
        };

        private readonly string[,] pathData =
        {
            { @"C:\this\is\a\test", @"C:\this\is\a", "test" },
            { @"test\junk.file", "test", "junk.file" },
            { "test.file", null, "test.file" },
            { @"\has\no\filename\", @"\has\no\filename", null },
            { null, null, null },
            { "", null, null }
        };

        private readonly string[] validDirectoryPath_invalidData =
        {
            "Invalid\"data",
            "invalid\tdata",
            "invalid\bdata",
            "invalid<data",
            "invalid>data",
            "invalid\u000ddata",
            "invalid|data"
        };

        private readonly string[] validDirectoryPath_validData =
        {
            @"C:\valid\data",
            @"valid#data",
            @"\valid.data"
        };

        private readonly string[] validFileName_invalidData =
        {
            "Invalid\"data",
            "invalid\tdata",
            "invalid\bdata",
            "invalid<data",
            "invalid>data",
            "invalid\u001bdata",
            "invalid|data",
            @"invalid\data",
            "invalid:data"
        };

        private readonly string[] validFileName_validData =
        {
            "valid.name",
            "valid$name",
            "valid#name",
            "valid[name]"
        };

        private PrivateType pt = new PrivateType(typeof(FileOps));

        #endregion Private Fields

        #region Public Methods

        [TestMethod]
        [TestCategory("FileOps")]
        public void CombinePath_InvalidPath()
        {
            for (int i = 0; i < combinePath_InvalidPath_data.GetUpperBound(0); i++)
            {
                string part1 = combinePath_InvalidPath_data[i, 0];
                string part2 = combinePath_InvalidPath_data[i, 1];
                CombinePath_InvalidPath_test(i, part1, part2);
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void CombinePath_ValidPath()
        {
            for (int i = 0; i < combinePath_ValidPath_data.GetUpperBound(0); i++)
            {
                string part1 = combinePath_ValidPath_data[i, 0];
                string part2 = combinePath_ValidPath_data[i, 1];
                string expected = combinePath_ValidPath_data[i, 2];
                string result = null;
                try
                {
                    result = FileOps.CombinePath(part1, part2);
                    Assert.AreEqual(expected, result);
                }
                catch (Exception e)
                {
                    string msg = $"CombinePath_ValidPath test failed\nindex={i}\nPart1: {part1}\nPart2: {part2}" +
                        $"\nResult: {result}\nException: {e.Message}";
                    Assert.Fail(msg);
                }
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void CreateFile_invalidPath()
        {
            string filePath = @"C:\this\path\not\found\create.file";
            try
            {
                FileOps.CreateFile(filePath);
            }
            catch (FileOperationException)
            {
                return;
            }
            string msg = $"CreateFile_invalidPath failed to throw an exception\nFile path: {filePath}";
            Assert.Fail(msg);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void CreateFile_nullPath()
        {
            string filePath = null;
            try
            {
                FileOps.CreateFile(filePath);
            }
            catch (FileOperationException)
            {
                return;
            }
            string msg = "CreateFile_nullPath failed to throw an exception";
            Assert.Fail(msg);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void CreateFile_valid()
        {
            string filePath = Directory.GetCurrentDirectory() + @"\create.file";
            if (File.Exists(filePath)) File.Delete(filePath);
            FileOps.CreateFile(filePath);
            Assert.IsTrue(File.Exists(filePath));
            File.Delete(filePath);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void FileMustExist_fileDoesNotExist()
        {
            string filePath = null;
            try
            {
                filePath = Path.GetFullPath(@"NonExistantFile.junk");
            }
            catch (Exception e)
            {
                string msg = $"FileMustExist_fileDoesNotExst test failed\nUnable to get full file path: {filePath}" +
                    $"\n{e.Message}";
                Assert.Fail(msg);
            }
            try
            {
                FileOps.FileMustExist(filePath);
            }
            catch (FileOpenException)
            {
                return;
            }
            string msg2 = $"FileMustExist_fileDoesNotExist test failed\nFile path: {filePath}";
            Assert.Fail(msg2);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void FileMustExist_fileExists()
        {
            string filePath = null;
            try
            {
                filePath = Path.GetFullPath(@"TestFile.junk");
                StreamWriter sw = new StreamWriter(File.Create(filePath));
                sw.Close();
                sw.Dispose();
            }
            catch (Exception e)
            {
                string msg = $"FileMustExist_fileExists test failed\nUnable to create test file: {filePath}" +
                    $"\n{e.Message}";
                Assert.Fail(msg);
            }
            FileOps.FileMustExist(filePath);
            File.Delete(filePath);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void FileMustNotExist_fileDoesNotExist()
        {
            string filePath = null;
            try
            {
                filePath = Path.GetFullPath(@"NonExistantFile.junk");
            }
            catch (Exception e)
            {
                string msg = $"FileMustNotExist_fileDoesNotExst test failed" +
                    $"\nUnable to get full file path: {filePath}\n{e.Message}";
                Assert.Fail(msg);
            }
            FileOps.FileMustNotExist(filePath);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void FileMustNotExist_fileExists()
        {
            string filePath = null;
            try
            {
                filePath = Path.GetFullPath(@"TestFile.junk");
                StreamWriter sw = new StreamWriter(File.Create(filePath));
                sw.Close();
                sw.Dispose();
            }
            catch (Exception e)
            {
                string msg = $"FileMustNotExist_fileExists test failed\nUnable to create test file: {filePath}" +
                    $"\n{e.Message}";
                Assert.Fail(msg);
            }
            try
            {
                FileOps.FileMustNotExist(filePath);
            }
            catch (FileOpenException)
            {
                File.Delete(filePath);
                return;
            }
            try
            {
                File.Delete(filePath);
            }
            catch { }
            string msg2 = $"FileMustNotExist_fileExists test failed\nFile path: {filePath}";
            Assert.Fail(msg2);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void FindEndOfDirectoryPath_valid()
        {
            for (int i = 0; i < findEndOfDirectoryPath_pathData.Length; i++)
            {
                int result = FindEndOfDirectoryPath(findEndOfDirectoryPath_pathData[i]);
                Assert.AreEqual(findEndOfDirectoryPath_expectedData[i], result);
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void GetAbsoluteFilePath_InvalidPath()
        {
            for (int i = 0; i < getAbsoluteFilePath_InvalidPath_data.Length; i++)
            {
                string path = getAbsoluteFilePath_InvalidPath_data[i];
                GetAbsoluteFilePath_InvalidPath_test(i, path);
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void GetAbsoluteFilePath_ValidPath()
        {
            string currentPath = Directory.GetCurrentDirectory();
            string dir1 = @"this\is\a\test";
            string expected = String.Concat(currentPath, @"\", dir1);
            string result = null;
            try
            {
                result = FileOps.GetAbsoluteFilePath(dir1);
            }
            catch (Exception e)
            {
                string msg = $"GetAbsoluteFilePath_ValidPath test failed\nException: {e.Message}" +
                    $"\nCurrent working directory: {currentPath}\nFile path: {dir1}\nExpected result: {expected}" +
                    $"\nActual result: {result}";
                Assert.Fail(msg);
            }
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void GetDirectoryPath_valid()
        {
            for (int i = 0; i < pathData.GetLength(0); i++)
            {
                string result = GetDirectoryPath_test(pathData[i, 0]);
                Assert.AreEqual(pathData[i, 1], result);
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void GetFileName_valid()
        {
            for (int i = 0; i < pathData.GetLength(0); i++)
            {
                string result = GetFileName_test(pathData[i, 0]);
                Assert.AreEqual(pathData[i, 2], result);
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void TruncateFile_valid()
        {
            string filePath = Directory.GetCurrentDirectory() + @"\truncate.txt";
            long fileLength = 9999;
            try
            {
                using (FileStream fs = File.OpenWrite(filePath))
                {
                    Byte[] text = new UTF8Encoding(true).GetBytes("This is a test");
                    fs.Write(text, 0, text.Length);
                }
            }
            catch (Exception e)
            {
                string msg = $"TruncateFile_valid test failed\nException: {e.Message}\nFile path: {filePath}";
                Assert.Fail(msg);
            }
            FileOps.TruncateFile(filePath);
            using (FileStream fs = File.OpenRead(filePath))
            {
                fileLength = fs.Length;
            }
            Assert.AreEqual(0, fileLength);
            File.Delete(filePath);
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void ValidDirectoryPath_invalidPaths()
        {
            for (int i = 0; i < validDirectoryPath_invalidData.Length; i++)
            {
                Assert.IsFalse(FileOps.ValidDirectoryPath(validDirectoryPath_invalidData[i]));
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void ValidDirectoryPath_validPaths()
        {
            for (int i = 0; i < validDirectoryPath_validData.Length; i++)
            {
                Assert.IsTrue(FileOps.ValidDirectoryPath(validDirectoryPath_validData[i]));
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void ValidFileName_invalidName()
        {
            for (int i = 0; i < validFileName_invalidData.Length; i++)
            {
                Assert.IsFalse(FileOps.ValidFileName(validFileName_invalidData[i]));
            }
        }

        [TestMethod]
        [TestCategory("FileOps")]
        public void ValidFileName_validName()
        {
            for (int i = 0; i < validFileName_validData.Length; i++)
            {
                Assert.IsTrue(FileOps.ValidFileName(validFileName_validData[i]));
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void CombinePath_InvalidPath_test(int i, string part1, string part2)
        {
            string result = null;
            try
            {
                result = FileOps.CombinePath(part1, part2);
            }
            catch (FilePathException)
            {
                return;
            }
            string msg = $"CombinePath_InvalidPath test failed\nTest index: {i}\nPart1: {part1}" +
                $"\nPart2: {part2}\nResult: {result}";
            Assert.Fail(msg);
        }

        private int FindEndOfDirectoryPath(string filePath)
        {
            return Convert.ToInt32(pt.InvokeStatic("FindEndOfDirectoryPath", filePath));
        }

        private void GetAbsoluteFilePath_InvalidPath_test(int i, string path)
        {
            string result = null;
            try
            {
                result = FileOps.GetAbsoluteFilePath(path);
            }
            catch (FilePathException)
            {
                return;
            }
            string msg = $"GetAbsoluteFilePath_InvalidPath test failed\nTest index: {i}\nPath: {path}" +
                $"\nResult: {result}";
            Assert.Fail(msg);
        }

        private string GetDirectoryPath_test(string filePath)
        {
            return FileOps.GetDirectoryPath(filePath);
        }

        private string GetFileName_test(string filePath)
        {
            return FileOps.GetFileName(filePath);
        }

        #endregion Private Methods
    }
}