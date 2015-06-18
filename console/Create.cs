using System;
using System.IO;
using FubuCore.CommandLine;
using model;

namespace console {
	[CommandDescription("Create the specified database from scripts.", Name="create")]
	public class Create : FubuCommand<DbCommandInput> {


		public override bool Execute(DbCommandInput input)
		{
			var db = input.CreateDatabase();
			if (!Directory.Exists(db.Dir)){
				Console.WriteLine("Snapshot dir {0} does not exist.", db.Dir);
				return false;
			}

			bool overwrite = input.OverwriteFlag;

			if (DBHelper.DbExists(db.ConnectionString) && !input.OverwriteFlag) {
				Console.WriteLine("{0} {1} already exists do you want to drop it? (Y/N)",
					input.Server, input.Database);

				var answer = char.ToUpper(Convert.ToChar(Console.Read()));
				while (answer != 'Y' && answer != 'N') {
					answer = char.ToUpper(Convert.ToChar(Console.Read()));
				}
				if (answer == 'N') {
					Console.WriteLine("create command cancelled.");
					return false;
				}
				overwrite = true;
			}

			try {
				db.CreateFromDir(overwrite);
				Console.WriteLine("Database created successfully.");
			}
			catch (BatchSqlFileException ex) {
				Console.WriteLine(@"Create completed with the following errors:");
				foreach (SqlFileException e in ex.Exceptions) {
					Console.WriteLine(@"{0}(Line {1}): {2}",
						e.FileName.Replace("/", "\\"), e.LineNumber, e.Message);
				}
			}
			catch (SqlFileException ex) {
				Console.Write(@"A SQL error occurred while executing scripts.
{0}(Line {1}): {2}", ex.FileName.Replace("/", "\\"), ex.LineNumber, ex.Message);
				return false;
			}

			return true;
		}
	}
}