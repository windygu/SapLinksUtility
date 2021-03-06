﻿using System;
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
        private const string TableNameHeader = ControlFieldPrefix + "TNH"; // Table Name Header control
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
        public string FullFilePath { get; private set; }

        /// <summary>
        /// Property that returns the text file name associated with this data table
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Property that returns the directory path for the text file
        /// </summary>
        public string FileDirectoryPath
        {
            get
            {
                return _file.DirectoryPath;
            }
        }

        /// <summary>
        /// Private property that returns the state of the text file (INITIAL, OPEN, or CLOSED)
        /// </summary>
        private FileState FileState
        {
            get
            {
                return _file.State;
            }
        }

        /// <summary>
        /// Private property that returns the mode of the text file (READ, WRITE, or INITIAL)
        /// </summary>
        private FileMode FileMode
        {
            get
            {
                return _file.Mode;
            }
        }

        /// <summary>
        /// Private property that returns the current position within the text file
        /// </summary>
        private int FilePosition
        {
            get
            {
                return _file.Position;
            }
        }

        /// <summary>
        /// Private property that returns the number of lines in the text file
        /// </summary>
        private int FileLineCount
        {
            get
            {
                return _file.Count;
            }
        }

        /// <summary>
        /// Private property that returns a "true" if we have reached the end of the text file
        /// </summary>
        private bool EndOfFile
        {
            get
            {
                return _file.EndOfFile;
            }
        }

        /// <summary>
        /// Property that returns the number of field names for this data table
        /// </summary>
        public int FieldCount
        {
            get
            {
                return _fieldNames.Count;
            }
        }

        /// <summary>
        /// Property that returns the number of data rows for this data table
        /// </summary>
        public int RowCount
        {
            get
            {
                return _dataRows.Count;
            }
        }

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
            FullFilePath = _file.FilePath;
            FileName = _file.FileName;
            _fieldNames = new List<string>();
            _dataRows = new List<string[]>();
            // Open the associated text file for reading
            _file.OpenForRead(FullFilePath);
            if (FileLineCount == 0)
            {
                // If the associated text file is empty, then initialize the file by writing the list of field names
                // to the file.
                Initialize(fieldNames);
            }
            else
            {
                // If the associated text file contains data, then load that data into the DataTable
                Load();
                // Verify that the list of field names returned from the text file matches the expected list of
                // fields names passed to this constructor.
                ValidateTableFieldNameList(fieldNames);
            }
        }

        /// <summary>
        /// Add a new data row to the table. Returns "true" if successful. Returns "false" if there is already a row
        /// in the table with a matching unique key.
        /// </summary>
        /// <param name="dataRow">A DataRow object representing the data row to be added to the table</param>
        /// <returns>Returns "true" if the data row is successfully added to the table. Otherwise,
        /// returns "false".</returns>
        public bool AddRow(DataRow dataRow)
        {
            // Get the list of field names from the data row
            string[] fieldNames = dataRow.FieldList;
            // Verify that the data row contains all of the correct fields for this data table
            ValidateDataRowFieldNames(fieldNames);
            if (dataRow.KeyCount > 0)
            {
                string[] keys = dataRow.KeyList; // List of key field names
                string[] keyValues = new string[keys.Length]; // Values of the key fields
                int[] tableKeyFieldIndex = GetFieldIndexValues(keys); // Get indexes to table key fields
                // Retrieve the value from the data row for each key field
                for (int i = 0; i < keys.Length; i++)
                {
                    keyValues[i] = dataRow[keys[i]];
                }
                if (RowWithKeyExists(tableKeyFieldIndex, keyValues)) return false;
            }
            int[] tableFieldIndex = GetFieldIndexValues(fieldNames);
            string[] dataRowValues = new string[fieldNames.Length];
            for (int i = 0; i < fieldNames.Length; i++)
            {
                dataRowValues[tableFieldIndex[i]] = dataRow[fieldNames[i]];
            }
            _dataRows.Add(dataRowValues);
            return true;
        }

        private int[] GetFieldIndexValues(string[] fieldNames)
        {
            int[] fieldIndexList = new int[fieldNames.Length];
            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (_fieldNames.Contains(fieldNames[i]))
                {
                    fieldIndexList[i] = _fieldNames.FindIndex(delegate (string fieldName)
                    {
                        return (fieldName == fieldNames[i]);
                    });
                }
                else
                {
                    // TODO: Throw an exception
                }
            }
            return fieldIndexList;
        }

        /// <summary>
        /// Scan the data rows in the table to see if any of them contain a specified set of key field values.
        /// Return "true" if any data row matches all of the key field values. Otherwise return "false".
        /// </summary>
        /// <param name="fieldIndex">Array contains a list of index values corresponding to the key fields</param>
        /// <param name="fieldValue">Array contains the key field values to search for</param>
        /// <returns>Returns "true" if any data row matches all of the key field values</returns>
        private bool RowWithKeyExists(int[] fieldIndex, string[] fieldValue)
        {
            // Search all of the data rows in the table
            foreach (string[] dataRow in _dataRows)
            {
                // Start out by assuming the current data row is a match
                bool match = true;
                // Check all of the key fields
                for (int i = 0; i < fieldIndex.Length; i++)
                {
                    // If the key field value in the current data row doesn't match the value being searched on,
                    // set the match indicator to "false" and go check the next data row.
                    if (dataRow[fieldIndex[i]] != fieldValue[i])
                    {
                        match = false;
                        break;
                    }
                }
                // If we reach this point, then all of the key field values in the data row match. Return "true".
                if (match) return true;
            }
            // If we reach this point then none of the data rows were a match. Return "false".
            return false;
        }

        /// <summary>
        /// Validate that the list of field names in a data row exactly matches the list of field names for this table.
        /// Throw an exception if there is any discrepancy.
        /// </summary>
        /// <param name="dataRowFieldNames"></param>
        private void ValidateDataRowFieldNames(string[] dataRowFieldNames)
        {
            if (dataRowFieldNames.Length == 0)
            {
                throw new DataRowException("Data row contains no fields");
            }
            // Verify that each of the fields in the data row are also fields in the table
            foreach (string fieldName in dataRowFieldNames)
            {
                if (_fieldNames.Contains(fieldName)) continue;
                // Throw an exception if the data row field name doesn't exist in the table
                else
                {
                    string msg = $"Data row field name \"{fieldName}\" doesn't exist in table \"{Name}\"";
                    throw new DataRowException(msg);
                }
            }
            // Verify that each of the table field names also appear in the data row
            foreach (string fieldName in _fieldNames)
            {
                if (dataRowFieldNames.Contains(fieldName)) continue;
                // Throw an exception if the data row doesn't contain all of the fields that are in the table
                else
                {
                    string msg = $"Table \"{Name}\" field \"{fieldName}\" is missing in the data row";
                    throw new DataRowException(msg);
                }
            }
        }

        /// <summary>
        /// Verify that the list of field names that were read in from the text file exactly matches the expected
        /// list of field names that was passed to the constructor.
        /// </summary>
        /// <param name="fieldNames"></param>
        private void ValidateTableFieldNameList(string[] fieldNames)
        {
            // Check to see that each of the field names that were read in from the text file are in the list of
            // expected field names
            foreach (string fieldName in _fieldNames)
            {
                if (fieldNames.Contains(fieldName)) continue;
                // Throw an exception if the text file contains any field names that aren't in the list of
                // expected field names
                else
                {
                    string msg = $"Table \"{Name}\" - Unexpected field name \"{fieldName}\" found in file " +
                        $"\"{FileName}\"";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
            // Verify that each of the names in the expected list of field names was found in the text file
            foreach (string fieldName in fieldNames)
            {
                if (_fieldNames.Contains(fieldName)) continue;
                // Throw an exception if we didn't find all of the expected field names in the text file
                else
                {
                    string msg = $"Table \"{Name}\" - Expected field name \"{fieldName}\" is missing in file " +
                        $"\"{FileName}\"";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
        }

        /// <summary>
        /// Initialize a new file by writing the list of field names to the file
        /// </summary>
        /// <param name="fieldNames"></param>
        protected void Initialize(string[] fieldNames)
        {
            // Add the list of field names to the field name list
            foreach (string fieldName in fieldNames)
            {
                _fieldNames.Add(fieldName);
            }
            // Save the table to the text file
            Save();
        }

        /// <summary>
        /// Load the contents of the associated text file into the DataTable
        /// </summary>
        protected void Load()
        {
            // Prepare the text file for loading. Close it if it is open for writing. Open it for reading.
            PrepareFileForLoad();
            // Clear the field name list and data row list
            _fieldNames.Clear();
            _dataRows.Clear();
            // Throw an exception if the text file is empty. At a minimum it should contain a list of field names.
            if (FileLineCount == 0)
            {
                string msg = $"File \"{FileName}\" of table \"{Name}\" is empty";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Load the table name from the text file
            LoadTableName();
            // Load the field names from the text file
            LoadFieldNames();
            // Load the data rows from the text file
            LoadDataRows();
            // We should be at the end of the text file now. Throw an exception if there are more lines in the file.
            if (!EndOfFile)
            {
                string fileData = _file.ReadLine();
                string msg = $"Table \"{Name}\" - End of file \"{FileName}\" not reached after the End Data " +
                    $"List control. Line {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Read the table name header from the text file. Throw an exception if the name found in the file
        /// doesn't match the name of this table.
        /// </summary>
        private void LoadTableName()
        {
            string fileData = _file.ReadLine();
            // Throw an exception if the line is null (this shouldn't happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line found in file \"{FileName}\" when " +
                    $"Table Name Header was expected";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Split the line into individual fields that are delimited by the delimiter character
            string[] tableNameHeader = fileData.Split(DelimChar);
            // Throw an exception if the line doesn't contain the Table Name Header control
            if (tableNameHeader[0] != TableNameHeader)
            {
                string msg = $"Table \"{Name}\" - Missing the Table Name Header control in file \"{FileName}\"" +
                    $"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Throw an exception if there isn't exactly one parameter following the Table Name Header control
            if (tableNameHeader.Length != 2)
            {
                string msg = $"Table \"{Name}\" - Invalid number of parameters in the Table Name Header control " +
                    $"in file \"{FileName}\"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            string tableName = tableNameHeader[1];
            // Throw an exception if the table name returned from the text file doesn't match the name of this table
            if (tableName != Name)
            {
                string msg = $"Incorrect table name found in file \"{FileName}\".\nExpected \"{Name}\" " +
                    $"but found \"{tableName}\".";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Throw an exception if we have reached the end of the text file
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - End of file \"{FileName}\" reached immediately after the " +
                    "Table Name Header line";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Retrieve the expected number of data rows from the Begin Data List control line in the text file
        /// </summary>
        /// <returns>Returns an integer value equal to the number of data rows in the text file</returns>
        private int GetRowCount()
        {
            // Throw an exception if we reach the end of the file before reading any data rows
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - End of file \"{FileName}\" reached before any data rows " +
                        $"were read";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Read the next line from the file. Should be the Begin Data Rows control line.
            string fileData = _file.ReadLine();
            // Throw an exception if the line is null (this shouldn't happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line found in file \"{FileName}\" when " +
                    $"Begin Data List was expected";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Split the line into individual fields that are delimited by the delimiter character
            string[] begindDataListLine = fileData.Split(DelimChar);
            // The next line in the file should be the Begin Data List control. Throw an exception if it is not.
            if (begindDataListLine[0] != BeginDataList)
            {
                string msg = $"Table \"{Name}\" - Missing the Begin Data List control in file \"{FileName}\"" +
                    $"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The Begin Data List control should be followed by a value which equals the number of data rows
            if (begindDataListLine.Length != 2)
            {
                string msg = $"Table \"{Name}\" - Invalid number of parameters in the Begin Field List control " +
                    $"in file \"{FileName}\"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            int totalRows = 0; // Total number of data rows we expect to find in the text file
            // Convert the row count value from a string value to an integer value
            try
            {
                totalRows = Convert.ToInt32(begindDataListLine[1]);
            }
            // Throw an exception if we are unable to convert the string value to an integer
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to convert the data row count string value to an integer " +
                    $"in file \"{FileName}\"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg, e)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return totalRows;
        }

        /// <summary>
        /// Load the next data row from the text file. Return "true" if there are no more rows to load.
        /// Otherwise, return "false".
        /// </summary>
        /// <param name="totalRows">The expected number of data rows in the text file</param>
        /// <returns>Returns "true" if there are no more rows to load</returns>
        protected bool LoadNextDataRow(int totalRows)
        {
            // Throw an exception if we reach the end of the file too soon
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - Reached the end of file \"{FileName}\" before finding " +
                    $"the End Data List control.\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            string fileData = _file.ReadLine();
            // Throw an exception if a null line is returned from the file (this should never happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line returned from file \"{FileName}\" when " +
                    $"reading the data rows.\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // If we have reached the End Data List control, then we have processed all of the data rows.
            // Return "true" to let the caller know we are done.
            if (fileData == EndDataList) return true;
            // There shouldn't be any other control lines in the file other than the End Data List control.
            // Throw an exception if we come across any other control field.
            else if (fileData.Substring(0, ControlFieldPrefix.Length) == ControlFieldPrefix)
            {
                string msg = $"Table \"{Name}\" - Unexpected control field in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The first two characters on the line should be the Data Row Prefix. Discard the prefix and separate
            // the rest of the line into individual fields. Keep a count of total data rows processed thus far.
            else if (fileData.Substring(0, DataRowPrefix.Length) == DataRowPrefix)
            {
                string[] dataFields = (fileData.Substring(DataRowPrefix.Length)).Split(DelimChar);
                // The number of fields should match the number of field names. Throw an exception if it doesn't.
                if (dataFields.Length != FieldCount)
                {
                    string msg = $"Table \"{Name}\" - Data row {RowCount + 1} in file \"{FileName}\" " +
                        $"contains an incorrect number of fields.\nExpected {FieldCount} fields, but found " +
                        $"{dataFields.Length}.\nLine {FilePosition} from file contains this ->\n{fileData}";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
                _dataRows.Add(dataFields);
            }
            // If the first two characters on the line aren't a Control Field Prefix or a Data Row Prefix, then
            // the file has been corrupted. Throw an exception.
            else
            {
                string msg = $"Table \"{Name}\" - Data Row Prefix missing in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return false; // Return "false" to let the caller know we're not done yet
        }

        /// <summary>
        /// Load all of the data rows from the text file into the data table
        /// </summary>
        private void LoadDataRows()
        {
            int totalRows = GetRowCount(); // Get the total number of expected data rows
            // Read all of the data rows from the text file and add the row data to the data list
            while (!LoadNextDataRow(totalRows)) { }
            // Verify that we have read the expected number of data rows. Throw an exception if we haven't.
            if (RowCount != totalRows)
            {
                string rowDiff = (RowCount < totalRows) ? "Fewer" : "More";
                string msg = $"Table \"{Name}\" - {rowDiff} rows were found than were expected.\n " +
                    $"Found {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Read the Begin Field List control line from the text file and retrieve the expected number of field names
        /// </summary>
        /// <returns>Returns an integer value equal to the expected number of fields</returns>
        private int GetFieldCount()
        {
            // Read the first line from the text file
            string fileData = _file.ReadLine();
            // Throw an exception if the first line is null (this shouldn't happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - First line of file \"{FileName}\" is null";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Split the line into individual fields that are delimited by the delimiter character
            string[] beginFieldListLine = fileData.Split(DelimChar);
            // The first line in the file should be the Begin Field List control. Throw an exception if it is not.
            if (beginFieldListLine[0] != BeginFieldList)
            {
                string msg = $"Table \"{Name}\" - Missing the Begin Field List control in file \"{FileName}\"" +
                    $"\nThe first line in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The Begin Field List control should be followed by a value which equals the number of field names
            if (beginFieldListLine.Length != 2)
            {
                string msg = $"Table \"{Name}\" - Missing the field count value of the Begin Field List control " +
                    $"in file \"{FileName}\"\nThe first line in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            int totalFields = 0; // Total number of fields we expect to find in the text file
            // Convert the field count value from a string value to an integer value
            try
            {
                totalFields = Convert.ToInt32(beginFieldListLine[1]);
            }
            // Throw an exception if we are unable to convert the string value to an integer
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to convert the field count string value to an integer " +
                    $"in file \"{FileName}\"\nThe first line in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg, e)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return totalFields;
        }

        /// <summary>
        /// Load the next field name from the text file. Return "true" if we have reached the End Field List control.
        /// Otherwise, return "false".
        /// </summary>
        /// <param name="totalFields">The total number of field names expected to be found in the text file</param>
        /// <returns>Returns "true" when we have reached the end of the list of field names</returns>
        private bool LoadNextFieldName(int totalFields)
        {
            // Throw an exception if we reach the end of the file too soon
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - Reached the end of file \"{FileName}\" before finding " +
                    $"the End Field List control.\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            string fileData = _file.ReadLine();
            // Throw an exception if a null line is returned from the file (this should never happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line returned from file \"{FileName}\" when " +
                    $"reading the list of fields.\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Throw an exception if there are any field delimiter characters on the current line.
            // The field name lines shouldn't contain any delimiter characters.
            if (fileData.Contains(Delimiter))
            {
                string msg = $"Table \"{Name}\" - Delimiter found in field list of file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // If we have reached the End Field List control, then we have processed all of the field names.
            // Return "true" to let the caller know we are done.
            if (fileData == EndFieldList) return true;
            // The next control field in the file should be the End Field List control which we just checked for
            // above. Throw an exception if we come across any other control field.
            else if (fileData.Substring(0, ControlFieldPrefix.Length) == ControlFieldPrefix)
            {
                string msg = $"Table \"{Name}\" - Unexpected control field in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The first two characters on the line should be the Data Row Prefix. Discard the prefix and add the
            // field name to the field name list. Keep a tally of how many fields we've processed thus far.
            else if (fileData.Substring(0, DataRowPrefix.Length) == DataRowPrefix)
            {
                _fieldNames.Add(fileData.Substring(DataRowPrefix.Length));
            }
            // If the first two characters on the line aren't a Control Field Prefix or a Data Row Prefix, then
            // the file has been corrupted. Throw an exception.
            else
            {
                string msg = $"Table \"{Name}\" - Data Row Prefix missing in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return false;
        }

        /// <summary>
        /// Load all of the field names from the text file into the data table
        /// </summary>
        private void LoadFieldNames()
        {
            int totalFields = GetFieldCount(); // Get the expected number of field names from the text file
            // Read all of the field name lines from the text file and add the field names to the field list
            while (!LoadNextFieldName(totalFields)) { }
            // We expect the number of field names read from the text file to match the number that was specified
            // on the Begin Field List control line
            if (FieldCount == totalFields)
            {
                // Throw an exception if there are an invalid number of fields. The number of fields must be in the
                // range from 1 to MaxFields.
                if (FieldCount <= 0 || FieldCount > MaxFields)
                {
                    string msg = $"Table \"{Name}\" - Invalid number of fields in file \"{FileName}\".\n" +
                        $"Found {FieldCount} of {totalFields} fields. The number found should be between " +
                        $"1 and {MaxFields}.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
            // Throw an exception if the number of fields found doesn't match the number specified on the
            // Begin Field List control line.
            else
            {
                string fieldMsg = (FieldCount < totalFields) ? "Too few" : "Too many";
                string msg = $"Table \"{Name}\" - {fieldMsg} fields found in file \"{FileName}\".\n" +
                        $"Found {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Prepare the text file for loading into the data table. If the file is open, close it. If the file is
        /// in the initial state or closed, open it for reading. If the file is already opened for reading, reset
        /// the file pointer back to the beginning of the file.
        /// </summary>
        private void PrepareFileForLoad()
        {
            if (FileState == FileState.OPEN)
            {
                // Close the text file if it is currently open for writing
                if (FileMode == FileMode.WRITE)
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
                        string msg = $"Unable to reset file \"{FileName}\" for data table \"{Name}\"";
                        throw new DataTableLoadException(msg, e)
                        {
                            TableName = Name,
                            FileName = FileName,
                            DirectoryPath = FileDirectoryPath
                        };
                    }
                }
            }
            // Open the text file for reading if it is currently closed or in an initial state
            if (FileState == FileState.INITIAL || FileState == FileState.CLOSED)
            {
                try
                {
                    _file.OpenForRead(FullFilePath);
                }
                // Throw an exception if we can't open the file for reading
                catch (Exception e)
                {
                    string msg = $"Unable to open file \"{FileName}\" to load table \"{Name}\"";
                    throw new DataTableLoadException(msg, e)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
            // Throw an exception if the file isn't opened for reading.
            // Note: This exception shouldn't occur.
            if (FileState != FileState.OPEN || FileMode != FileMode.READ)
            {
                string msg = $"Expected file state {FileState.OPEN.ToString()} and " +
                    $"file mode {FileMode.READ.ToString()}\nbut found file state {FileState.ToString()} and " +
                    $"file mode {FileMode.ToString()}";
                throw new UnexpectedFileStateException(msg)
                {
                    ExpectedState = FileState.OPEN.ToString(),
                    ExpectedMode = FileMode.READ.ToString(),
                    ActualState = FileState.ToString(),
                    ActualMode = FileMode.ToString()
                };
            }
        }

        /// <summary>
        /// Save the table contents to the text file
        /// </summary>
        public void Save()
        {
            // Open the text file for writing
            PrepareFileForSave();
            // Write the table name header to the text file
            WriteTableNameHeader();
            // Write the list of fields to the text file
            WriteFieldList();
            // Write the data rows to the text file
            WriteDataRows();
            // Close the file to save the changes
            _file.Close();
        }

        /// <summary>
        /// Write the Table Name Header to the text file
        /// </summary>
        private void WriteTableNameHeader()
        {
            _file.WriteLine(TableNameHeader + Delimiter + Name);
        }

        /// <summary>
        /// Write the list of field names to the text file
        /// </summary>
        private void WriteFieldList()
        {
            // Write the Begin Field List line to the text file
            string beginFieldList = BeginFieldList + Delimiter + Convert.ToString(FieldCount);
            _file.WriteLine(beginFieldList);
            // Write the list of field names to the text file
            if (FieldCount > 0)
            {
                foreach (string fieldName in _fieldNames)
                {
                    _file.WriteLine(DataRowPrefix + fieldName);
                }
            }
            // Write the End Field List line to the text file
            _file.WriteLine(EndFieldList);
        }

        /// <summary>
        /// Write all of the data rows to the text file
        /// </summary>
        private void WriteDataRows()
        {
            // Write the Begin Data List line to the text file
            string beginDataList = BeginDataList + Delimiter + Convert.ToString(RowCount);
            _file.WriteLine(beginDataList);
            // Write the data rows to the text file
            if (RowCount > 0)
            {
                foreach (string[] dataRow in _dataRows)
                {
                    string line = dataRow[0];
                    if (dataRow.Length > 1)
                    {
                        for (int i = 1; i < dataRow.Length; i++)
                        {
                            line += Delimiter + dataRow[i];
                        }
                    }
                    _file.WriteLine(DataRowPrefix + line);
                }
            }
            // Write the End Data List line to the text file
            _file.WriteLine(EndDataList);
        }

        /// <summary>
        /// Open the text file for writing. Throw an exception if the file is already open for writing. Close the
        /// file if it is open for reading and then open it for writing.
        /// </summary>
        private void PrepareFileForSave()
        {
            // Close the file if it is open
            if (FileState == FileState.OPEN)
            {
                // Throw an exception if the file is currently open for writing
                if (FileMode == FileMode.WRITE)
                {
                    // Close the file first to save any changes
                    _file.Close();
                    string msg = $"Table \"{Name}\" - Unable to save table to file \"{FileName}\" because the file " +
                        $"was already open for writing.";
                    throw new DataTableSaveException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
                // Close the file
                _file.Close();
            }
            // Open the file for writing
            try
            {
                _file.OpenForWrite(FullFilePath);
            }
            catch (Exception e)
            {
                // Throw an exception if we are unable to open the text file for writing
                string msg = $"Table \"{Name}\" - Unable to open file \"{FileName}\" for saving the table.";
                throw new DataTableSaveException(msg, e)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }
    }
}
