using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// The DataRow object can be used for transferring a single data row between a data table and another object
    /// </summary>
    public class DataRow
    {
        #region Private Fields

        private Dictionary<string, string> _row; // Internally a data row is represented as a Dictionary object
        private List<string> _keyFields; // A list of field names whose values uniquely identify a single data row

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Default constructor that creates a DataRow with an unspecified number of fields
        /// </summary>
        public DataRow()
        {
            _row = new Dictionary<string, string>();
            _keyFields = new List<string>();
        }

        /// <summary>
        /// Constructor that creates a DataRow with the specified number of fields
        /// </summary>
        /// <param name="size">
        /// Number of fields contained in this DataRow
        /// </param>
        public DataRow(int size)
        {
            _row = new Dictionary<string, string>(size);
            _keyFields = new List<string>(size);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Property that returns the number of fields in the DataRow
        /// </summary>
        public int FieldCount => _row.Count();

        /// <summary>
        /// Property that returns the number of key fields for this DataRow
        /// </summary>
        public int KeyCount => _keyFields.Count();

        /// <summary>
        /// Property that returns a list of the field names contained in the DataRow as an array of strings
        /// </summary>
        public string[] FieldList
        {
            get
            {
                return _row.Keys.ToArray();
            }
        }

        /// <summary>
        /// Return the list of key field names as an array of strings
        /// </summary>
        public string[] KeyList
        {
            get
            {
                return _keyFields.ToArray();
            }
        }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Indexer that returns or sets the field contents for the specified field name
        /// </summary>
        /// <param name="fieldName">
        /// This is the field name we wish to retrieve
        /// </param>
        /// <returns>
        /// Returns the field contents for the specified field name
        /// </returns>
        public string this[string fieldName]
        {
            get
            {
                // Throw an exception if an invalid field name is specified
                if (!_row.ContainsKey(fieldName))
                {
                    string msg = $"Unable to return field value. Field name \"{fieldName}\" doesn't exist in " +
                        $"this data row.";
                    throw new DataRowException(msg);
                }
                return _row[fieldName];
            }
            set
            {
                // Throw an exception if an invalid field name is specified
                if (!_row.ContainsKey(fieldName))
                {
                    string msg = $"Unable to set field value. Field name \"{fieldName}\" doesn't exist in " +
                        $"this data row.";
                    throw new DataRowException(msg);
                }
                _row[fieldName] = value;
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Add a new field to the DataRow
        /// </summary>
        /// <param name="fieldName">
        /// The key is the field name
        /// </param>
        /// <param name="fieldValue">
        /// This is the field contents
        /// </param>
        /// <param name="isKeyField">
        /// Key field indicator. Defaults to "false". Set "true" for key fields.
        /// </param>
        public void AddField(string fieldName, string fieldValue, bool isKeyField = false)
        {
            // Throw an exception if the DataRow already contains the specified field name
            if (_row.ContainsKey(fieldName))
            {
                string msg = $"Field name \"{fieldName}\" has already been added to this data row";
                throw new DataRowException(msg);
            }
            // Add the new field to the data row
            _row.Add(fieldName, fieldValue);
            // If this is a key field, then add the field name to the list of key fields
            if (isKeyField)
            {
                _keyFields.Add(fieldName);
            }
        }

        #endregion Public Methods
    }
}