using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace FileIO.Tests
{
    [TestClass]
    public class FileOpsTests
    {
        private string[,] combinePath_ValidPath_data =
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

        private string[,] combinePath_InvalidPath_data =
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

        private string[] getAbsoluteFilePath_InvalidPath_data =
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
    }
}