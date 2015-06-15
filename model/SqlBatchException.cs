using System;
using System.Data.SqlClient;

namespace model {
	public class SqlBatchException : Exception {
		private readonly int lineNumber;

		private readonly string message;

		public SqlBatchException(SqlException ex, int prevLinesInBatch)
			: base("", ex) {
			lineNumber = ex.LineNumber + prevLinesInBatch;
			message = ex.Message;
		}

		public int LineNumber {
			get { return lineNumber; }
		}

		public override string Message {
			get { return message; }
		}
	}
}