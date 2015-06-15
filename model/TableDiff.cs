using System.Collections.Generic;
using System.Text;

namespace model {
	public class TableDiff {
		public List<Column> ColumnsAdded = new List<Column>();
		public List<ColumnDiff> ColumnsDiff = new List<ColumnDiff>();
		public List<Column> ColumnsDroped = new List<Column>();

		public List<Constraint> ConstraintsAdded = new List<Constraint>();
		public List<Constraint> ConstraintsChanged = new List<Constraint>();
		public List<Constraint> ConstraintsDeleted = new List<Constraint>();
		public string Name;
		public string Owner;

		public bool IsDiff {
			get {
				return ColumnsAdded.Count + ColumnsDroped.Count + ColumnsDiff.Count + ConstraintsAdded.Count +
				       ConstraintsChanged.Count + ConstraintsDeleted.Count > 0;
			}
		}

		public string Script() {
			var text = new StringBuilder();

			foreach (Column c in ColumnsAdded) {
				text.AppendFormat("ALTER TABLE [{0}].[{1}] ADD {2}\r\n", Owner, Name, c.Script());
			}

			foreach (Column c in ColumnsDroped) {
				text.AppendFormat("ALTER TABLE [{0}].[{1}] DROP COLUMN [{2}]\r\n", Owner, Name, c.Name);
			}

			foreach (ColumnDiff c in ColumnsDiff) {
				text.AppendFormat("ALTER TABLE [{0}].[{1}] ALTER COLUMN {2}\r\n", Owner, Name, c.Script());
			}
			return text.ToString();
		}
	}
}