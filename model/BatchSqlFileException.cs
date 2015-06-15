using System;
using System.Collections.Generic;

namespace model {
	public class BatchSqlFileException : Exception {
		public List<SqlFileException> Exceptions { get; set; }
	}
}