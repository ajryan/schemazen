using System;
using System.Collections.Generic;
using System.Text;

namespace model {
	public class ForeignKey : Scriptable {
		public bool Check;
		public List<string> Columns;
		public string Name;
		public string OnDelete;
		public string OnUpdate;
		public List<string> RefColumns;
		public string RefTableName;
		public string RefTableOwner;
		public string TableName;
		public string TableOwner;

		public ForeignKey(string tableName, string tableOwner, string name,
			string onUpdate = "", string onDelete = "", bool isDisabled = false,
			string columns = "", string refTableName = "", string refTableOwner = "", string refColumns = "") {
			TableName = tableName;
			TableOwner = tableOwner;
			Name = name;
			OnUpdate = onUpdate;
			OnDelete = onDelete;
			Check = !isDisabled;
			Columns = new List<string>(columns.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			RefTableName = refTableName;
			RefTableOwner = refTableOwner;
			RefColumns = new List<string>(refColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
		}

		public string CheckText => Check ? "CHECK" : "NOCHECK";

		private void AssertArgNotNull(object arg, string argName) {
			if (arg == null) {
				throw new ArgumentNullException($"Unable to Script FK {Name}. {argName} must not be null.");
			}
		}

		public override string BaseFileName => $"{TableOwner}.{TableName}.ForeignKeys";

		public override string ScriptCreate() {
			AssertArgNotNull(TableName, "TableName");
			AssertArgNotNull(Columns, "Columns");
			AssertArgNotNull(RefTableName, "RefTableName");
			AssertArgNotNull(RefColumns, "RefColumns");

			var text = new StringBuilder();
			text.AppendFormat("ALTER TABLE [{0}].[{1}] WITH {2} ADD CONSTRAINT [{3}]\r\n", TableOwner, TableName, CheckText,
				Name);
			text.AppendFormat("   FOREIGN KEY([{0}]) REFERENCES [{1}].[{2}] ([{3}])\r\n", string.Join("], [", Columns.ToArray()),
				RefTableOwner, RefTableName, string.Join("], [", RefColumns.ToArray()));
			if (!string.IsNullOrEmpty(OnUpdate)) {
				text.AppendFormat("   ON UPDATE {0}\r\n", OnUpdate);
			}
			if (!string.IsNullOrEmpty(OnDelete)) {
				text.AppendFormat("   ON DELETE {0}\r\n", OnDelete);
			}
			if (!Check) {
				text.AppendFormat("   ALTER TABLE [{0}].[{1}] NOCHECK CONSTRAINT [{2}]\r\n",
					TableOwner, TableName, Name);
			}
			return text.ToString();
		}

		public string ScriptDrop() {
			return $"ALTER TABLE [{TableOwner}].[{TableName}] DROP CONSTRAINT [{Name}]\r\n";
		}
	}
}