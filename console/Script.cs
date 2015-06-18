using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FubuCore.CommandLine;
using model;

namespace console {
	public class ScriptInput : DbCommandInput {
		[Description("A comma separated list of tables to export data from.")]
		public string DataTablesFlag { get; set; }

		[Description("A regular expression pattern that matches tables to export data from.")]
		public string DataTablesPatternFlag { get; set; }
	}

	[CommandDescription("Generate scripts for the specified database.", Name="script")]
	public class Script : FubuCommand<ScriptInput> {
		protected string DataTables { get; set; }
		protected string DataTablesPattern { get; set; }

		public override bool Execute(ScriptInput input)
		{
			if (!input.OverwriteFlag && Directory.Exists(input.ScriptDir)) {
				Console.Write("{0} already exists do you want to replace it? (Y/N)", input.ScriptDir);
				var key = Console.ReadKey();
				if (key.Key != ConsoleKey.Y) {
					return false;
				}
				Console.WriteLine();
			}

			var db = input.CreateDatabase();
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

			db.ScriptToDir();

			Console.WriteLine("Snapshot successfully created at " + db.Dir);
			return true;
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
