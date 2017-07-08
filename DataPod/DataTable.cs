using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileIO;

namespace Data
{
    public class DataTable
    {
        private const string ControlFieldPrefix = "##"; // String that appears at the start of every control field
        private const string DataRowPrefix = "->"; // String that appears at the start of every data row in the file
        private const string BeginFieldList = ControlFieldPrefix + "BFL"; // Start list of field names
        private const string EndFieldList = ControlFieldPrefix + "EFL"; // End list of field names
        private const string BeginDataList = ControlFieldPrefix + "BDL"; // Beginning of data rows
        private const string EndDataList = ControlFieldPrefix + "EDL"; // Ending of data rows (end of file)
        private const string Delimiter = "\t"; // Delimiter character for separating fields in a data row
        private const char DelimChar = '\t'; // Delimiter character
        private const int MaxFields = 100; // Specifies the maximum number of fields that there can be in a file

        /// <summary>
        /// Property that returns the name of this DataTable
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Property that returns the full file path of the associated text file for this DataTable
        /// </summary>
        public string FilePath { get; private set; }

        private List<string> _fieldNames; // List of field names for this DataTable
        private List<string[]> _dataRows; // List of data rows that make up this DataTable
        private EncodedTextFile _file; // Associated text file used to persist this DataTable

        /// <summary>
        /// Public constructor for the DataTable
        /// </summary>
        /// <param name="tableName">The identifying name of this DataTable</param>
        /// <param name="directoryPath">The directory path to the associated text file</param>
        /// <param name="fileName">The text file that is associated with this DataTable</param>
        /// <param name="fieldNames">The list of file names for each data row in the table</param>
        public DataTable(string tableName, string directoryPath, string fileName, string[] fieldNames)
        {
            Name = tableName;
            _file = new EncodedTextFile();
            // Create the associated text file for the DataTable if it doesn't already exist.
            // The created file will be empty
            _file.CreateIfFileDoesNotExist(directoryPath, fileName);
            FilePath = _file.FilePath;
            _fieldNames = new List<string>();
            _dataRows = new List<string[]>();
            // Open the associated text file for reading
            _file.OpenForRead(FilePath);
            if (_file.Count == 0)
            {
                // If the associated text file is empty, then initialize the file by writing the list of field names
                // to the file.
                Initialize(fieldNames);
            }
            else
            {
                // If the associated text file contains data, then load that data into the DataTable
                Load();
            }
        }

        /// <summary>
        /// Initialize a new file by writing the list of field names to the file
        /// </summary>
        /// <param name="fieldNames"></param>
        protected virtual void Initialize(string[] fieldNames)
        {
            // First, close the file if it is open
            if (_file.State == FileState.OPEN)
            {
                _file.Close();
            }
            // Open the file for writing
            _file.OpenForWrite(FilePath);
            // Write the "beginning of field list" identifier to the file
            string beginFields = BeginFieldList + Delimiter + fieldNames.Length.ToString();
            _file.WriteLine(beginFields);
            // Write each of the field names to the file. Also add each field name to the list of valid field names
            // for this DataTable.
            foreach (string fieldName in fieldNames)
            {
                _fieldNames.Add(fieldName);
                _file.WriteLine(DataRowPrefix + fieldName);
            }
            // Write the "end of field list" identifier to the file
            _file.WriteLine(EndFieldList);
            // Write the "beginning of data rows" identifier to the file
            string beginData = BeginDataList + Delimiter + Convert.ToString(0);
            _file.WriteLine(beginData);
            // Write the "end of data rows" identifier to the file
            _file.WriteLine(EndDataList);
            // Close the file to save the changes
            _file.Close();
        }

        /// <summary>
        /// Load the contents of the associated text file into the DataTable
        /// </summary>
        protected virtual void Load()
        {
            if (_file.State == FileState.OPEN)
            {
                // Close the text file if it is currently open for writing
                if (_file.Mode == FileMode.WRITE)
                {
                    _file.Close();
                }
                // Reset the position to the start of the file if it is open for reading
                else
                {
                    try
                    {
                        _file.Reset();
                    }
                    // Throw an exception if we are unable to reset the position pointer for the file
                    catch (Exception e)
                    {
                        string msg = $"Unable to reset file \"{_file.FileName}\" for data table \"{Name}\"";
                        throw new DataTableLoadException(msg, e)
                        {
                            TableName = Name,
                            FileName = _file.FileName,
                            DirectoryPath = _file.DirectoryPath
                        };
                    }
                }
            }
            // Open the text file for reading if it is currently closed or in an initial state
            if (_file.State == FileState.INITIAL || _file.State == FileState.CLOSED)
            {
                try
                {
                    _file.OpenForRead(FilePath);
                }
                // Throw an exception if we can't open the file for reading
                catch (Exception e)
                {
                    string msg = $"Unable to open file \"{_file.FileName}\" to load table \"{Name}\"";
                    throw new DataTableLoadException(msg, e)
                    {
                        TableName = Name,
                        FileName = _file.FileName,
                        DirectoryPath = _file.DirectoryPath
                    };
                }
            }
            // Throw an exception if the file isn't opened for reading.
            // Note: This exception shouldn't occur.
            if (_file.State != FileState.OPEN || _file.Mode != FileMode.READ)
            {
                string msg = $"Expected file state {FileState.OPEN.ToString()} and " +
                    $"file mode {FileMode.READ.ToString()}\nbut found file state {_file.State.ToString()} and " +
                    $"file mode {_file.Mode.ToString()}";
                throw new UnexpectedFileStateException(msg)
                {
                    ExpectedState = FileState.OPEN.ToString(),
                    ExpectedMode = FileMode.READ.ToString(),
                    ActualState = _file.State.ToString(),
                    ActualMode = _file.Mode.ToString()
                };
            }
            // Clear the field name list and data row list
            _fieldNames.Clear();
            _dataRows.Clear();
            // Throw an exception if the text file is empty. At a minimum it should contain a list of field names.
            if (_file.Count == 0)
            {
                string msg = $"File \"{_file.FileName}\" of table \"{Name}\" is empty";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = _file.FileName,
                    DirectoryPath = _file.DirectoryPath
                };
            }
            string fileData = null;
            // Read the first line from the text file
            fileData = _file.ReadLine();
            // Throw an exception if the first line is null (this shouldn't happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - First line of file \"{_file.FileName}\" is null";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = _file.FileName,
                    DirectoryPath = _file.DirectoryPath
                };
            }
            // Split the line into individual fields that are delimited by the delimiter character
            string[] firstLine = fileData.Split(DelimChar);
            // The first line in the file should be the Begin Field List control. Throw an exception if it is not.
            if (firstLine[0] != BeginFieldList)
            {
                string msg = $"Table \"{Name}\" - Missing the Begin Field List control in file \"{_file.FileName}\"" +
                    $"\nThe first line in the file contains this ->\n{firstLine}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = _file.FileName,
                    DirectoryPath = _file.DirectoryPath
                };
            }
            // The Begin Field List control should be followed by a value which equals the number of field names
            if (firstLine.Length != 2)
            {
                string msg = $"Table \"{Name}\" - Missing the field count value of the Begin Field List control " +
                    $"in file \"{_file.FileName}\"\nThe first line in the file contains this ->\n{firstLine}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = _file.FileName,
                    DirectoryPath = _file.DirectoryPath
                };
            }
            int totalFields = 0; // Total number of fields we expect to find in the text file
            // Convert the field count value from a string value to an integer value
            try
            {
                totalFields = Convert.ToInt32(firstLine[1]);
            }
            // Throw an exception if we are unable to convert the string value to an integer
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to convert the field count string value to an integer " +
                    $"in file \"{_file.FileName}\"\nThe first line in the file contains this ->\n{firstLine}";
                throw new DataTableLoadException(msg, e)
                {
                    TableName = Name,
                    FileName = _file.FileName,
                    DirectoryPath = _file.DirectoryPath
                };
            }
            int fieldCount = 0; // Actual number of fields found in the text file
            bool done = false; // Loop control. Set to "true" when we reach the end of the field list.
            // Read all of the field name lines from the text file and add the field names to the field list
            while (!done)
            {
                // Throw an exception if we reach the end of the file too soon
                if (_file.EndOfFile)
                {
                    string msg = $"Table \"{Name}\" - Reached the end of file \"{_file.FileName}\" before finding " +
                        $"the End Field List control.\nFound {fieldCount} of {totalFields} fields.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = _file.FileName,
                        DirectoryPath = _file.DirectoryPath
                    };
                }
                fileData = _file.ReadLine();
                // Throw an exception if a null line is returned from the file (this should never happen)
                if (fileData == null)
                {
                    string msg = $"Table \"{Name}\" - Null line returned from file \"{_file.FileName}\" when " +
                        $"reading the list of fields.\nFound {fieldCount} of {totalFields} fields.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = _file.FileName,
                        DirectoryPath = _file.DirectoryPath
                    };
                }
                // Throw an exception if there are any field delimiter characters on the current line.
                // The field name lines shouldn't contain any delimiter characters.
                if (fileData.Contains(Delimiter))
                {
                    int i = _file.Position + 1;
                    string msg = $"Table \"{Name}\" - Delimiter found in field list of file \"{_file.FileName}\".\n" +
                        $"Line {i} from file ->\n{fileData}\nFound {fieldCount} of {totalFields} fields.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = _file.FileName,
                        DirectoryPath = _file.DirectoryPath
                    };
                }
                // If we have reached the End Field List control, then we have processed all of the field names. Set
                // the flag to indicate we're done and return to the top of the loop.
                if (fileData == EndFieldList)
                {
                    done = true;
                    continue;
                }
                // The next control field in the file should be the End Field List control which we just checked for
                // above. Throw an exception if we come across any other control field.
                else if (fileData.Substring(0,ControlFieldPrefix.Length) == ControlFieldPrefix)
                {
                    int i = _file.Position + 1;
                    string msg = $"Table \"{Name}\" - Unexpected control field in file \"{_file.FileName}\".\n" +
                        $"Line {i} from file ->\n{fileData}\nFound {fieldCount} of {totalFields} fields.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = _file.FileName,
                        DirectoryPath = _file.DirectoryPath
                    };
                }
                // The first two characters on the line should be the Data Row Prefix. Discard the prefix and add the
                // field name to the field name list. Keep a tally of how many fields we've processed thus far.
                else if (fileData.Substring(0,DataRowPrefix.Length) == DataRowPrefix)
                {
                    _fieldNames.Add(fileData.Substring(DataRowPrefix.Length));
                    fieldCount++;
                }
                // If the first two characters on the line aren't a Control Field Prefix or a Data Row Prefix, then
                // the file has been corrupted. Throw an exception.
                else
                {
                    int i = _file.Position + 1;
                    string msg = $"Table \"{Name}\" - Data Row Prefix missing in file \"{_file.FileName}\".\n" +
                        $"Line {i} from file ->\n{fileData}\nFound {fieldCount} of {totalFields} fields.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = _file.FileName,
                        DirectoryPath = _file.DirectoryPath
                    };
                }
            }
            // We expect the number of field names read from the text file to match the number that was specified
            // on the Begin Field List control line
            if (fieldCount == totalFields)
            {
                // Throw an exception if there are an invalid number of fields. The number of fields must be in the
                // range from 1 to MaxFields.
                if (fieldCount <= 0 || fieldCount > MaxFields)
                {
                    string msg = $"Table \"{Name}\" - Invalid number of fields in file \"{_file.FileName}\".\n" +
                        $"Found {fieldCount} of {totalFields} fields. The number found should be between " +
                        $"1 and {MaxFields}.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = _file.FileName,
                        DirectoryPath = _file.DirectoryPath
                    };
                }
            }
            // Throw an exception if the number of fields found doesn't match the number specified on the
            // Begin Field List control line.
            else
            {
                string fieldMsg = (fieldCount < totalFields) ? "Too few" : "Too many";
                string msg = $"Table \"{Name}\" - {fieldMsg} fields found in file \"{_file.FileName}\".\n" +
                        $"Found {fieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = _file.FileName,
                    DirectoryPath = _file.DirectoryPath
                };
            }
            // Throw an exception if we reach the end of the file before reading any data rows
            if (_file.EndOfFile)
            {
                string msg = $"Table \"{Name}\" - End of file \"{_file.FileName}\" reached before any data rows " +
                        $"were read";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = _file.FileName,
                    DirectoryPath = _file.DirectoryPath
                };
            }
            // Read the next line from the file. Should be the Begin Data Rows control line.
            fileData = _file.ReadLine();
        }
    }
}
