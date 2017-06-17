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
            string result;
            try
            {
                result = (pt.InvokeStatic("DecodeTextString", etf, line)).ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        private string EncodeTextString(EncodedTextFile etf, string line)
        {
            string result;
            try
            {
                result = (pt.InvokeStatic("EncodeTextString", etf, line)).ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
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

        private readonly string[,] decodeTextString_oneBitFlipped_data =
        {
            { "\u1842", "\u0023" }, // bit 1 changed
            { "\u11b9", "\u005b" }, // bit 2 changed
            { "\u08ce", "\u006a" }, // bit 3 changed
            { "\u095a", "\u0022" }, // bit 4 changed
            { "\u0b36", "\u0096" }, // bit 5 changed
            { "\u131e", "\u009e" }, // bit 6 changed
            { "\u172f", "\u00bf" }, // bit 7 changed
            { "\u12d7", "\u00a7" }, // bit 8 changed
            { "\u0f45", "\u00a5" }, // bit 9 changed
            { "\u0354", "\u0024" }, // bit 10 changed
            { "\u1577", "\u0037" }, // bit 11 changed
            { "\u0866", "\u0036" }, // bit 12 changed
            { "\u026a", "\u00ba" }, // bit 13 changed
            { "\u34d8", "\u0068" }, // bit 14 changed
            { "\u4198", "\u0048" }, // bit 15 changed
            { "\u99db", "\u006b" }  // bit 16 changed
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

        [TestMethod]
        [TestCategory("EncodedTextFile")]
        public void DecodeTextString_valid()
        {
            EncodedTextFile etf = new EncodedTextFile();
            for (int i = 0; i < encodeTextString_valid_data.GetLength(0); i++)
            {
                string result = DecodeTextString(etf, encodeTextString_valid_data[i, 1]);
                if (result != encodeTextString_valid_data[i, 0])
                {
                    ushort rslt = (ushort)(result[0]);
                    ushort expt = (ushort)((encodeTextString_valid_data[i, 0])[0]);
                    string msg = $"DecodeTextString_valid test failed\nTest index: {i}\nActual result: " +
                        $"{rslt:X4}\nExpected result: {expt:X4}";
                    Assert.Fail(msg);
                }
            }
        }

        [TestMethod]
        [TestCategory("EncodedTextFile")]
        public void DecodeTextString_oneBitFlipped()
        {
            EncodedTextFile etf = new EncodedTextFile();
            for (int i = 0; i < decodeTextString_oneBitFlipped_data.GetLength(0); i++)
            {
                string result = DecodeTextString(etf, decodeTextString_oneBitFlipped_data[i, 0]);
                if (result != decodeTextString_oneBitFlipped_data[i, 1])
                {
                    ushort rslt = (ushort)(result[0]);
                    ushort expt = (ushort)((decodeTextString_oneBitFlipped_data[i, 1])[0]);
                    string msg = $"DecodeTextString_oneBitFlipped test failed\nTest index: {i}\nActual result: " +
                        $"{rslt:X4}\nExpected result: {expt:X4}";
                    Assert.Fail(msg);
                }
            }
        }
    }
}
