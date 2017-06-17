using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    class UnexpectedFileStateException : ApplicationException
    {
        private const string _baseMessage = "Unexpected file state exception";
        public UnexpectedFileStateException() : base(_baseMessage)
        {
        }
        public UnexpectedFileStateException(string msg) :
            base($"{_baseMessage}: {msg}")
        {
            Message = msg;
        }
        public UnexpectedFileStateException(string msg, Exception inner) :
            base($"{_baseMessage}: {msg}", inner)
        {
            Message = msg;
        }
        public string ExpectedState { get; set; }
        public string ExpectedMode { get; set; }
        public string ActualState { get; set; }
        public string ActualMode { get; set; }
        public override string Message { get; }
    }
}
