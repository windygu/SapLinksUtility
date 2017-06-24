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
        private const string BeginFieldList = "##BFL"; // String that appears at the beginning of a list of field names
        private const string EndFieldList = "##EFL"; // String that appears at the end of a list of field names
        private const string BeginDataList = "##BDL"; // String that appears at the beginning of the data rows
        private const string EndDataList = "##EDL"; // String that appears at the end of the data rows (end of file)

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
            string beginFields = BeginFieldList + " " + fieldNames.Length.ToString();
            _file.WriteLine(beginFields);
            // Write each of the field names to the file. Also add each field name to the list of valid field names
            // for this DataTable.
            foreach (string fieldName in fieldNames)
            {
                _fieldNames.Add(fieldName);
                _file.WriteLine(fieldName);
            }
            // Write the "end of field list" identifier to the file
            _file.WriteLine(EndFieldList);
            // Write the "beginning of data rows" identifier to the file
            string beginData = BeginDataList + " 0";
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
                        string msg = $"Unable to reset file {_file.FileName} for data table {Name}";
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
                    string msg = $"Unable to open file {_file.FileName} to load table {Name}";
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
            // Return without doing anything else if the text file is empty
            if (_file.Count == 0) return;
            // The first line in the file should be the "beginning of field list" identifier
            string[] firstLine = _file.ReadLine().Split(' ');
            if (firstLine.Length != 2)
            {
                // TODO: Throw an exception if there aren't two fields in the first record
            }
            if (firstLine[0] != BeginFieldList)
            {
                // TODO: Throw an exception if the first line doesn't contain the correct identifier
            }
            int totalFields = Convert.ToInt32(firstLine[1]);
            int fieldCount = 0;
        }
    }
}
