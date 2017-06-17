using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DataRow
    {
        private Dictionary<String, String> _row;
        public DataRow()
        {
            _row = new Dictionary<string, string>();
        }
        public DataRow(int size)
        {
            _row = new Dictionary<string, string>(size);
        }
        public void Add(string key, string data)
        {
            if (_row.ContainsKey(key))
            {
                // throw exception
            }
            _row.Add(key, data);
        }
        public string GetValue(string key)
        {
            if (!_row.ContainsKey(key))
            {
                // throw exception
            }
            bool success = _row.TryGetValue(key, out string data);
            if (!success)
            {
                // throw exception
            }
            return data;
        }
        public string this [string key]
        {
            get
            {
                if (!_row.ContainsKey(key))
                {
                    // throw exception
                }
                return _row[key];
            }
            set
            {
                if (!_row.ContainsKey(key))
                {
                    // throw exception
                }
                _row[key] = value;
            }
        }
    }
}
