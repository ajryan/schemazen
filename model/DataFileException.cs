using System;

namespace model {
	public class DataFileException : Exception {
		private readonly string _fileName;
		private readonly int _lineNumber;
		private readonly string _message;

		public DataFileException(string message, string fileName, int lineNumber) {
			_message = message;
			_fileName = fileName;
			_lineNumber = lineNumber;
		}

		public override string Message {
			get { return _message; }
		}

		public string FileName {
			get { return _fileName; }
		}

		public int LineNumber {
			get { return _lineNumber; }
		}
	}
}