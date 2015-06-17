using System;
using System.IO;
using System.Linq;
using model;

namespace console {
	public class Script : DbCommand {

		protected string DataTables { get; set; }
		protected string DataTablesPattern { get; set; }

		public Script() : base(
			"Script", "Generate scripts for the specified database.") {
			HasOption(
				"dataTables=",
				"A comma separated list of tables to export data from.",
				o => DataTables = o);
			HasOption(
				"dataTablesPattern=",
				"A regular expression pattern that matches tables to export data from.",
				o => DataTablesPattern = o);
		}

		public override int Run(string[] args) {
			if (!Overwrite && Directory.Exists(ScriptDir))
			{
				Console.Write("{0} already exists do you want to replace it? (Y/N)", ScriptDir);
				var key = Console.ReadKey();
				if (key.Key != ConsoleKey.Y)
				{
					return 1;
				}
				Console.WriteLine();
			}

			var db = CreateDatabase();
			db.Load();

			if (!string.IsNullOrEmpty(DataTables)) {
				HandleDataTables(db, DataTables);
			}

			if (!string.IsNullOrEmpty(DataTablesPattern)) {
				var tables = db.FindTablesRegEx(DataTablesPattern);
				foreach (var t in tables) {
					if (db.DataTables.Contains(t)) continue;
					db.DataTables.Add(t);
				}
			}

			db.ScriptToDir(Overwrite);

			Console.WriteLine("Snapshot successfully created at " + db.Dir);
			return 0;
		}

		private static void HandleDataTables(Database db, string tableNames) {
			foreach (var tableName in tableNames.Split(',')) {
				var schema = "dbo";
				var name = tableName;
				if (tableName.Contains(".")) {
					schema = tableName.Split('.')[0];
					name = tableName.Split('.')[1];
				}
				var t = db.Tables.Find(name, schema);
				if (t == null) {
					Console.WriteLine(
						"warning: could not find data table {0}.{1}",
						schema, name);
				}
				if (db.DataTables.Contains(t)) continue;
				db.DataTables.Add(t);
			}
		}
	}
}