using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileIO;

namespace FileIOTest
{
    [TestClass]
    public class EncodedTextFileTests
    {
        private PrivateType pt = new PrivateType(typeof(EncodedTextFile));

        private string DecodeTextString(EncodedTextFile etf, string line)
        {
            return (pt.InvokeStatic("DecodeTextString", etf, line)).ToString();
        }

        private string EncodeTextString(EncodedTextFile etf, string line)
        {
            return (pt.InvokeStatic("EncodeTextString", etf, line)).ToString();
        }

        private readonly string[,] encodeTextString_valid_data =
        {
            { "\u0000", "\u0000" },
            { "\u00ff", "\u0eef" },
            { "\u00cc", "\u0b8c" },
            { "\u0055", "\u01a5" },
            { "\u000f", "\u010f" },
            { "\u00f0", "\u0fe0" },
            { "\u0052", "\u18b2" },
            { "\u00d3", "\u17a3" }
        };

        [TestMethod]
        [TestCategory("EncodedTextFile")]
        public void EncodeTextString_valid()
        {
            EncodedTextFile etf = new EncodedTextFile();
            for (int i = 0; i < encodeTextString_valid_data.GetLength(0); i++)
            {
                string result = EncodeTextString(etf, encodeTextString_valid_data[i, 0]);
                if (result != encodeTextString_valid_data[i,1])
                {
                    ushort rslt = (ushort)(result[0]);
                    ushort expt = (ushort)((encodeTextString_valid_data[i, 1])[0]);
                    string msg = $"EncodeTextString_valid test failed\nTest index: {i}\nActual result: " +
                        $"{rslt:X4}\nExpected result: {expt:X4}";
                    Assert.Fail(msg);
                }
            }
        }
    }
}
