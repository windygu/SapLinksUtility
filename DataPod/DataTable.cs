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
        private const string BeginFieldList = "##BFL";
        private const string EndFieldList = "##EFL";
        public string Name { get; private set; }
        public string FilePath { get; private set; }
        private List<string> _fieldNames;
        private List<string[]> _records;
        private EncodedTextFile _file;
        public DataTable(string podName, string directoryPath, string fileName, string[] fieldNames)
        {
            Name = podName;
            _file = new EncodedTextFile();
            _file.CreateIfFileDoesNotExist(directoryPath, fileName);
            FilePath = _file.FilePath;
            _fieldNames = new List<string>();
            _records = new List<string[]>();
            _file.OpenForRead(FilePath);
            if (_file.Count == 0)
            {
                Initialize(fieldNames);
            }
            else
            {
                Load();
            }
        }
        protected virtual void Initialize(string[] fieldNames)
        {
            if (_file.State == FileState.OPEN)
            {
                _file.Close();
            }
            _file.OpenForWrite(FilePath);
            _file.WriteLine(BeginFieldList);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                _fieldNames.Add(fieldNames[i]);
                _file.WriteLine(fieldNames[i]);
            }
            _file.WriteLine(EndFieldList);
            _file.Close();
        }
        protected virtual void Load()
        {
            if (_file.State == FileState.OPEN)
            {
                if (_file.Mode == FileMode.WRITE)
                {
                    _file.Close();
                }
            }
            if (_file.State == FileState.INITIAL || _file.State == FileState.CLOSED)
            {
                _file.OpenForRead(FilePath);
            }
            if (_file.State != FileState.OPEN || _file.Mode != FileMode.READ)
            {
                throw new UnexpectedFileStateException()
                {
                    ExpectedState = FileState.OPEN.ToString(),
                    ExpectedMode = FileMode.READ.ToString(),
                    ActualState = _file.State.ToString(),
                    ActualMode = _file.Mode.ToString()
                };
            }
        }
    }
}
