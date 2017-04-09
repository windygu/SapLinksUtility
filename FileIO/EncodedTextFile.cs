using System;
using System.IO;
using System.Text;

namespace FileIO
{
    /// <summary>
    /// Object represents an encoded text file. Implements IDataFile interface.
    /// </summary>
    internal class EncodedTextFile : TextFile, IDataFile
    {
        // The following constants are used as column indexes into the
        // codeTable
        private const int srcBit = 0;  // bit position in the plain text character (source)

        private const int tgtBit = 1;  // bit position in the encoded text character (target)

        // The following constants are used as indexes into both the codeTable
        // and the parityTable.
        private const int P0 = 1;  // Parity bit P0 (overall parity)

        private const int P1 = 2;  // Parity bit P1 (0x1)
        private const int P2 = 3;  // Parity bit P2 (0x2)
        private const int P4 = 4;  // Parity bit P4 (0x4)
        private const int P8 = 5;  // Parity bit P8 (0x8)

        // The codeTable is used for encoding/decoding text strings. Each row
        // in the table represents a bit position within a character value from
        // the source text string. Each column represents -
        // Column 0 - the bit position in the source character
        // Column 1 - the corresponding bit position in the target character
        // Column 2 - P1 parity value for this bit position
        // Column 3 - P2 parity value for this bit position
        // Column 4 - P4 parity value for this bit position
        // Column 5 - P8 parity value for this bit position
        private static readonly ushort[,] codeTable = new ushort[,]
        {
            { 0b1000_0000, 0b0010_0000_0000, 1, 2, 0, 0 },
            { 0b0100_0000, 0b0000_1000_0000, 1, 0, 4, 0 },
            { 0b0010_0000, 0b0000_0100_0000, 0, 2, 4, 0 },
            { 0b0001_0000, 0b0000_0010_0000, 1, 2, 4, 0 },
            { 0b0000_1000, 0b0000_0000_1000, 1, 0, 0, 8 },
            { 0b0000_0100, 0b0000_0000_0100, 0, 2, 0, 8 },
            { 0b0000_0010, 0b0000_0000_0010, 1, 2, 0, 8 },
            { 0b0000_0001, 0b0000_0000_0001, 0, 0, 4, 8 }
        };

        // The parityTable gives the bit positions for each of the parity bits.
        // The first element in the array isn't used. The remaining elements
        // represent parity bits P0, P1, P2, P4, and P8, in that order.
        private static readonly ushort[] parityTable = new ushort[]
        {
            0,
            0b0001_0000_0000_0000, // P0 - overall parity bit
            0b0000_1000_0000_0000, // P1 - parity bit 1
            0b0000_0100_0000_0000, // P2 - parity bit 2
            0b0000_0001_0000_0000, // P4 - Parity bit 4
            0b0000_0000_0001_0000  // P8 - parity bit 8
        };

        // The flipBit array is used for flipping the error bit in the decoded
        // text character when a single bit error is detected.
        //
        //                 --+----+----
        //                 12 4   8      parity bit positions
        //                   a bcd efgh  encoded text bit positions
        //                 --+----+----
        //                     abcdefgh  plain text bit positions
        //                 --+----+----
        // bit position    1 1    0   0
        //                 2 0    5   1
        private static readonly ushort[] flipBit = new ushort[]
        {
            0,              //    P1 parity bit
            0,              //    P2 parity bit
            0b1000_0000,    // a) encoded bit 10 / plain text bit 8
            0,              //    P4 parity bit
            0b0100_0000,    // b) encoded bit 8 / plain text bit 7
            0b0010_0000,    // c) encoded bit 7 / plain text bit 6
            0b0001_0000,    // d) encoded bit 6 / plain text bit 5
            0,              //    P8 parity bit
            0b0000_1000,    // e) encoded bit 4 / plain text bit 4
            0b0000_0100,    // f) encoded bit 3 / plain text bit 3
            0b0000_0010,    // g) encoded bit 2 / plain text bit 2
            0b0000_0001     // h) encoded bit 1 / plain text bit 1
        };

        /// <summary>
        /// The default constructor doesn't do anything other than to
        /// instantiate an EncodedTextFile object
        /// </summary>
        public EncodedTextFile() { }

        /// <summary>
        /// This is a finalizer that closes the encoded text file if it hasn't
        /// been closed already before handing things over to garbage
        /// collection
        /// </summary>
        ~EncodedTextFile()
        {
            if (State == FileState.OPEN) Close();
        }

        /// <summary>
        /// Open an existing encoded text file for reading.
        /// </summary>
        /// <param name="filePath">The file path and file name of the file to
        /// be opened</param>
        public override void OpenForRead(string filePath)
        {
            // Check to see if the file is already open. Throw an exception if
            // it is.
            if (State == FileState.OPEN)
            {
                throw new FileOpenException("File already open")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
            // Obtain the absolute file path. Throw an exception if the path is
            // invalid.
            ParseFilePath(filePath);
            // Throw an exception if the file doesn't exist.
            FileOps.FileMustExist(FilePath);
            State = FileState.OPEN;
            Mode = FileMode.READ;
            // Read the contents of the file into the _fileData collection
            try
            {
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    while (sr.Peek() >= 0) // Check to see if we've reached the end of the file
                    {
                        // Read the next line and store the decoded text string
                        _fileData.Add(DecodeTextString(sr.ReadLine()));
                    }
                    if (Count > 0) Position = 0; // Set the file pointer to the first line
                    else Position = -1;
                }
            }
            catch (Exception e)
            {
                string msg = $"Error reading line {this.Count + 1} from file";
                throw new FileIOException(msg, e)
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
        }

        /// <summary>
        /// Save the contents of the text file to disk
        /// </summary>
        public override void Save()
        {
            string filePath = FileOps.CombinePath(DirectoryPath, FileName);
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    foreach (string line in _fileData)
                    {
                        sw.WriteLine(EncodeTextString(line));
                    }
                }
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = NULL;
                throw new FileIOException("Unable to write file to disk", e)
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
        }

        /// <summary>
        /// Convert a line in plain text to an encoded text line with
        /// embedded error correcting parity bits. Return the encoded
        /// text line as a sting value.
        /// </summary>
        /// <param name="line">The text line to be encoded</param>
        /// <returns>Returns an encoded text line with embedded parity bits
        /// </returns>
        private string EncodeTextString(string line)
        {
            StringBuilder encodedLine = new StringBuilder();
            // Only the right-most 8 bits of each character in the text string
            // are relevant -
            //   1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16
            //  +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            //  |- |- |- |- |- |- |- |- |a |b |c |d |e |f |g |h |
            //  +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            //
            // These bits are placed into the output character like so -
            //   1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16
            //  +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            //  |0 |0 |0 |P0|P1|P2|a |P4|b |c |d |P8|e |f |g |h |
            //  +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            //
            // The following bit positions are parity bits.
            // Counting from the left as bit 1 -
            //   bit  4 - P0 - overall parity bit
            //   bit  5 - P1 - parity bit 1
            //   bit  6 - P2 - parity bit 2
            //   bit  8 - P4 - parity bit 4
            //   bit 12 - P8 - parity bit 8
            //
            // The parity bits are set so that the number of "1" bits in
            // each of the following groups of bits is an even number -
            // Group 0 = (P0, a, b, c, d, e, f, g, h)
            // Group 1 = (P1, a, b, d, e, g)
            // Group 2 = (P2, a, c, d, f, g)
            // Group 3 = (P4, b, c, d, h)
            // Group 4 = (P8, e, f, g, h)
            //
            // This is an implementation of the Hamming code
            // Encode each character in the text string
            foreach (char x in line)
            {
                // Throw an exception if the left-most 8 bits of the current
                // character contains anything other than zero
                if ((x & 0xff00) > 0)
                {
                    string msg = $"Unsupported Unicode character found: '{x}' ({(ushort)x:X4})";
                    throw new InvalidCharacterException(msg)
                    {
                        FilePath = DirectoryPath,
                        FileName = FileName
                    };
                }
                ushort encoded = 0;  // holds the encoded character as a ushort
                // Initialize the parity bits to zero
                ushort p0 = 0;  // overall parity bit
                ushort p1 = 0;  // parity bit for 0x01 position
                ushort p2 = 0;  // parity bit for 0x02 position
                ushort p4 = 0;  // parity bit for 0x04 position
                ushort p8 = 0;  // parity bit for 0x08 position
                // Evaluate the right-most 8 bits of the text character
                for (int i = 0; i < codeTable.GetLength(0); i++)
                {
                    // Determine if the current bit position holds a "1"
                    if ((x & codeTable[i, srcBit]) > 0)
                    {
                        // Transfer the "1" bit to the appropriate bit position
                        // in the encoded character
                        encoded += codeTable[i, tgtBit];
                        // Update the parity bits as needed
                        p0 ^= 1;
                        p1 ^= codeTable[i, P1];
                        p2 ^= codeTable[i, P2];
                        p4 ^= codeTable[i, P4];
                        p8 ^= codeTable[i, P8];
                    }
                }
                // Move the final parity bit values into the correct bit
                // positions in the encoded character.
                if (p0 > 0) { encoded += parityTable[P0]; }
                if (p1 > 0) { encoded += parityTable[P1]; }
                if (p2 > 0) { encoded += parityTable[P2]; }
                if (p4 > 0) { encoded += parityTable[P4]; }
                if (p8 > 0) { encoded += parityTable[P8]; }
                // Insert the encoded character into the encoded text line
                encodedLine.Append((char)encoded);
            }
            // Return the encoded text line as a string value
            return encodedLine.ToString();
        }

        /// <summary>
        /// Take an encoded text line and transform it back into its
        /// plain text form. Apply single-bit error correcting if
        /// necessary. Return the decoded text line as a string value.
        /// </summary>
        /// <param name="line">The encoded text line that needs to be decoded
        /// </param>
        /// <returns>Returns the decoded text line as a string value</returns>
        private string DecodeTextString(string line)
        {
            StringBuilder decodedLine = new StringBuilder();
            ushort p0 = 0;  // overall parity bit
            ushort p1 = 0;  // parity bit for 0x01 position
            ushort p2 = 0;  // parity bit for 0x02 position
            ushort p4 = 0;  // parity bit for 0x04 position
            ushort p8 = 0;  // parity bit for 0x08 position

            // Decode each character in the encoded text line.
            foreach (char x in line)
            {
                ushort decoded = 0;  // holds the decoded character as a ushort value

                // Initialize the parity bits to their respective values found in the encoded text line character
                if ((x & parityTable[P0]) > 0) p0 = 1; else p0 = 0;
                if ((x & parityTable[P1]) > 0) p1 = 1; else p1 = 0;
                if ((x & parityTable[P2]) > 0) p2 = 2; else p2 = 0;
                if ((x & parityTable[P4]) > 0) p4 = 4; else p4 = 0;
                if ((x & parityTable[P8]) > 0) p8 = 8; else p8 = 0;
                // Examine each of the bits that make up the plain text character
                for (int i = 0; i < codeTable.GetLength(0); i++)
                {
                    // If the bit is set, then toggle the appropriate parity bits
                    if ((x & codeTable[i, tgtBit]) > 0)
                    {
                        decoded += codeTable[i, srcBit];
                        p0 ^= 1;
                        p1 ^= codeTable[i, P1];
                        p2 ^= codeTable[i, P2];
                        p4 ^= codeTable[i, P4];
                        p8 ^= codeTable[i, P8];
                    }
                }
                // The badBit value will "point" to the bit in error if there is only one bit that is in error. A value
                // of zero signifies that there are no errors.
                ushort badBit = (ushort)(p1 + p2 + p4 + p8);
                if (badBit == 1 || badBit == 2 || badBit == 4 || badBit == 8)
                {
                    // If the badBit points to one of the p1, p2, p4, or p8 parity bits and the overall p0 parity bit
                    // is not zero, then two or more bits are corrupted. Throw an exception.
                    if (p0 != 0)
                    {
                        throw new CorruptedFileException("Encoded text file is corrupted")
                        {
                            FilePath = DirectoryPath,
                            FileName = FileName
                        };
                    }
                }
                else if (badBit > 0)
                {
                    // If the badBit is greater than zero and doesn't point at a parity bit, but the overall p0 parity
                    // bit is zero, then two or more bits are corrupted. Throw an exception.
                    if (p0 == 0)
                    {
                        throw new CorruptedFileException("Encoded text file is corrupted")
                        {
                            FilePath = DirectoryPath,
                            FileName = FileName
                        };
                    }
                    // There are twelve possible bit positions in the encoded text character containing the data bits
                    // from the original source character and the four parity bits p1, p2, p4, and p8. If the badBit
                    // points to anything higher than bit position 12, then throw an exception.
                    else if (badBit > 12)
                    {
                        throw new CorruptedFileException("Encoded text file is corrupted")
                        {
                            FilePath = DirectoryPath,
                            FileName = FileName
                        };
                    }
                    // If we reach this point, then there is one bit in error, and it is one of the 8 data bits from
                    // the original text character. Flip the bit that is in error to correct it.
                    decoded ^= flipBit[badBit - 1];
                }
                // Insert the decoded character into the decoded text line
                decodedLine.Append((char)decoded);
            }
            // Return the decoded text line as a string value
            return decodedLine.ToString();
        }

        /// <summary>
        /// ToString displays the characteristics of the text file
        /// </summary>
        /// <returns>Returns a string containing information about the file
        /// </returns>
        public override string ToString()
        {
            string directoryPath = DirectoryPath;
            if (directoryPath == null) directoryPath = NULL;
            string fileName = FileName;
            if (fileName == null) fileName = NULL;
            string fileState = State.ToString();
            string fileMode = Mode.ToString();
            return String.Format(
                "Encoded text file =========/n" +
                "File path: {0}\n" +
                "File name: {1}\n" +
                "File state: {2}\n" +
                "File mode: {3}\n" +
                "Number of lines: {4}" +
                "Current line: {5}",
                directoryPath, fileName, fileState, fileMode, Count, Position);
        }
    }
}