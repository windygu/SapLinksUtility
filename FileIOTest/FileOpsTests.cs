using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
                    string msg = String.Format("CombinePath_ValidPath exception\nindex={0}\npart1={1}\npart2={2}\n" +
                        "result={3}\nexception={4}", i, part1, part2, result, e.Message);
                    throw new Exception(msg);
                }
            }
        }

        private string[,] combinePath_InvalidPath_data =
        {
            { null, @"\\dir)2" },
            { @"dir1,", @"dir2" },
            { "dir(1", null },
            { "dir1", "dir\"2" },
            { "dir<1", "dir2" }
        };

        [TestMethod]
        public void CombinePath_InvalidPath()
        {
            for (int i = 0; i < combinePath_InvalidPath_data.GetUpperBound(0); i++)
            {
                string part1 = combinePath_InvalidPath_data[i, 0];
                string part2 = combinePath_InvalidPath_data[i, 1];
                string result = null;
                try
                {
                    result = FileOps.CombinePath(part1, part2);
                }
                catch (FilePathException e)
                {
                    return;
                }
                string msg = String.Format("CombinePath_InvalidPath didn't receive expected exception\n" +
                    "index={0}\npart1={1}\npart2={2}\nresult={3}", i, part1, part2, result);
                Assert.Fail(msg);
            }
        }
    }
}