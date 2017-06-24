using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DataRow
    {
        #region Private Fields

        private Dictionary<string, string> _row;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Default constructor that creates a DataRow with an unspecified number of fields
        /// </summary>
        public DataRow()
        {
            _row = new Dictionary<string, string>();
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
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Property that returns the number of fields in the DataRow
        /// </summary>
        public int FieldCount => _row.Count();

        /// <summary>
        /// Property that returns a list of the field names contained in the DataRow
        /// </summary>
        public List<string> FieldList
        {
            get
            {
                // Initialize an empty list of field names
                List<string> _fields = new List<string>(FieldCount);
                // Get the list of field names as a KeyCollection
                Dictionary<string, string>.KeyCollection _keys = _row.Keys;
                // If there is at least one field, then transfer the field names from the
                // KeyColletion to the List
                if (_keys.Count > 0)
                {
                    foreach (string key in _keys) _fields.Add(key);
                }
                // Return the list of field names
                return _fields;
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
                    // throw exception
                }
                return _row[fieldName];
            }
            set
            {
                // Throw an exception if an invalid field name is specified
                if (!_row.ContainsKey(fieldName))
                {
                    // throw exception
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
        public void Add(string fieldName, string fieldValue)
        {
            // Throw an exception if the DataRow already contains the specified field name
            if (_row.ContainsKey(fieldName))
            {
                // throw exception
            }
            _row.Add(fieldName, fieldValue);
        }

        /// <summary>
        /// Get the field contents for the specified field name
        /// </summary>
        /// <param name="fieldName">
        /// This is the field name
        /// </param>
        /// <returns>
        /// Returns the contents of the specified field
        /// </returns>
        public string GetValue(string fieldName)
        {
            // Throw an exception if the specified field name isn't valid
            if (!_row.ContainsKey(fieldName))
            {
                // throw exception
            }
            bool success = _row.TryGetValue(fieldName, out string fieldValue);
            // Throw an exception if we are unable to retrieve the field.
            // Note: This exception should never occur since we check for a valid field name earlier.
            if (!success)
            {
                // throw exception
            }
            // Return the field contents
            return fieldValue;
        }

        #endregion Public Methods
    }
}