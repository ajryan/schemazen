using System.Data.SqlClient;

namespace model {
	public class SqlFileException : SqlBatchException {
		private readonly string fileName;

		public SqlFileException(string fileName, SqlBatchException ex)
			: base((SqlException) ex.InnerException, ex.LineNumber - 1) {
			this.fileName = fileName;
		}

		public string FileName {
			get { return fileName; }
		}
	}
}