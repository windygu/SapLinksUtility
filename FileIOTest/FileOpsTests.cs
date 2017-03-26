using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileIO.Tests
{
    [TestClass]
    public class FileOpsTests
    {
        [TestMethod]
        public void CombinePath_ValidPath()
        {
            // Arrange
            string part1 = @"\dir1\dir2";
            string part2 = @"dir3\filename.txt";
            string expected = @"\dir1\dir2\dir3\filename.txt";
            // Act
            string result = FileOps.CombinePath(part1, part2);
            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}