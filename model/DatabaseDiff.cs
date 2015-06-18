using System.Collections.Generic;
using System.Text;

namespace model {
	public class DatabaseDiff {
		public Database Db;
		public List<ForeignKey> ForeignKeysAdded = new List<ForeignKey>();
		public List<ForeignKey> ForeignKeysDeleted = new List<ForeignKey>();
		public List<ForeignKey> ForeignKeysDiff = new List<ForeignKey>();
		public List<DbProperty> PropsChanged = new List<DbProperty>();

		public List<Routine> RoutinesAdded = new List<Routine>();
		public List<Routine> RoutinesDeleted = new List<Routine>();
		public List<Routine> RoutinesDiff = new List<Routine>();
		public List<Table> TablesAdded = new List<Table>();
		public List<Table> TablesDeleted = new List<Table>();
		public List<TableDiff> TablesDiff = new List<TableDiff>();

		public bool IsDiff => PropsChanged.Count > 0
		                      || TablesAdded.Count > 0
		                      || TablesDiff.Count > 0
		                      || TablesDeleted.Count > 0
		                      || RoutinesAdded.Count > 0
		                      || RoutinesDiff.Count > 0
		                      || RoutinesDeleted.Count > 0
		                      || ForeignKeysAdded.Count > 0
		                      || ForeignKeysDiff.Count > 0
		                      || ForeignKeysDeleted.Count > 0;

		public string DiffDescription => $@"{PropsChanged.Count} props changed
{TablesAdded.Count} tables added
{TablesDiff.Count} tables different
{TablesDeleted.Count} tables deleted
{RoutinesAdded.Count} routines added
{RoutinesDiff.Count} routines different
{RoutinesDeleted.Count} routines deleted
{ForeignKeysAdded.Count} foreign keys added
{ForeignKeysDiff.Count} foreign keys different
{ForeignKeysDeleted.Count} foreign keys deleted";

		public string Script() {
			var text = new StringBuilder();
			//alter database props
			//TODO need to check dependencies for collation change
			//TODO how can collation be set to null at the server level?
			if (PropsChanged.Count > 0) {
				text.Append(Database.ScriptProperties(PropsChanged));
				text.AppendLine("GO");
				text.AppendLine();
			}

			//delete foreign keys
			if (ForeignKeysDeleted.Count + ForeignKeysDiff.Count > 0) {
				foreach (ForeignKey fk in ForeignKeysDeleted) {
					text.AppendLine(fk.ScriptDrop());
				}
				//delete modified foreign keys
				foreach (ForeignKey fk in ForeignKeysDiff) {
					text.AppendLine(fk.ScriptDrop());
				}
				text.AppendLine("GO");
			}

			//add tables
			if (TablesAdded.Count > 0) {
				foreach (Table t in TablesAdded) {
					text.Append(t.ScriptCreate());
				}
				text.AppendLine("GO");
			}

			//modify tables
			if (TablesDiff.Count > 0) {
				foreach (TableDiff t in TablesDiff) {
					text.Append(t.Script());
				}
				text.AppendLine("GO");
			}

			//delete tables
			if (TablesDeleted.Count > 0) {
				foreach (Table t in TablesDeleted) {
					text.AppendLine(t.ScriptDrop());
				}
				text.AppendLine("GO");
			}

			//add foreign keys
			if (ForeignKeysAdded.Count + ForeignKeysDiff.Count > 0) {
				foreach (ForeignKey fk in ForeignKeysAdded) {
					text.AppendLine(fk.ScriptCreate());
				}
				//add modified foreign keys
				foreach (ForeignKey fk in ForeignKeysDiff) {
					text.AppendLine(fk.ScriptCreate());
				}
				text.AppendLine("GO");
			}

			//add & delete procs, functions, & triggers
			foreach (Routine r in RoutinesAdded) {
				text.AppendLine(r.ScriptCreate());
				text.AppendLine("GO");
			}
			foreach (Routine r in RoutinesDiff) {
				text.AppendLine(r.ScriptDrop());
				text.AppendLine("GO");
				text.AppendLine(r.ScriptCreate());
				text.AppendLine("GO");
			}
			foreach (Routine r in RoutinesDeleted) {
				text.AppendLine(r.ScriptDrop());
				text.AppendLine("GO");
			}

			return text.ToString();
		}
	}
}