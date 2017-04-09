using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace FileIO.Tests
{
    [TestClass]
    public class FileOpsTests
    {
        private PrivateType pt = new PrivateType(typeof(FileOps));

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

        private readonly string[,] combinePath_InvalidPath_data =
        {
            { null, @"\\dir)2" },
            { @"dir""1", @"dir2" },
            { "dir(1", null },
            { "dir1", "dir\"2" },
            { "dir<1", "dir2" }
        };

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
            catch { }
            File.Delete(filePath);
        }

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

        private readonly int[] findEndOfDirectoryPath_expectedData =
        {
            12, 0, 5, 4, -1, -1, -1
        };

        [TestMethod]
        [TestCategory("FileOps")]
        public void FindEndOfDirectoryPath_valid()
        {
            for (int i = 0; i < findEndOfDirectoryPath_pathData.Length; i++)
            {
                int result = FindEndOfDirectoryPath_valid_test(findEndOfDirectoryPath_pathData[i]);
                Assert.AreEqual(findEndOfDirectoryPath_expectedData[i], result);
            }
        }

        private int FindEndOfDirectoryPath_valid_test(string filePath)
        {
            return Convert.ToInt32(pt.InvokeStatic("FindEndOfDirectoryPath", filePath));
        }

        private readonly string[,] pathData =
        {
            { @"C:\this\is\a\test", @"C:\this\is\a", "test" },
            { @"test\junk.file", "test", "junk.file" },
            { "test.file", null, "test.file" },
            { @"\has\no\filename\", @"\has\no\filename", null },
            { null, null, null },
            { "", null, null }
        };

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

        private string GetDirectoryPath_test(string filePath)
        {
            return FileOps.GetDirectoryPath(filePath);
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

        private string GetFileName_test(string filePath)
        {
            return FileOps.GetFileName(filePath);
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
        }
    }
}