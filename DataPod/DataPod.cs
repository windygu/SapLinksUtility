using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileIO;

namespace DataPod
{
    public class DataPod
    {
        public string Name { get; private set; }
        private List<string> _fieldNames;
        private List<string[]> _records;
        private EncodedTextFile _file;
    }
}
