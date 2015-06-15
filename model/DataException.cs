using System;

namespace model {
	public class DataException : Exception {
		private readonly int _lineNumber;
		private readonly string _message;

		public DataException(string message, int lineNumber) {
			_message = message;
			_lineNumber = lineNumber;
		}

		public override string Message {
			get { return _message; }
		}

		public int LineNumber {
			get { return _lineNumber; }
		}
	}
}