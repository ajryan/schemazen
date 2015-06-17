using System;
using System.Threading.Tasks;
using ManyConsole;
using model;
using NDesk.Options;

namespace console {
	internal class Compare : ConsoleCommand {

		private string _source;
		private string _target;

		public Compare() {
			IsCommand("Compare", "Compare two databases.");
			Options = new OptionSet();
			SkipsCommandSummaryBeforeRunning();
			HasRequiredOption(
				"s|source=",
				"Connection string to a database to compare.",
				o => _source = o);
			HasRequiredOption(
				"t|target=",
				"Connection string to a database to compare.",
				o => _target = o);
		}

		public override int Run(string[] remainingArguments) {
			var sourceDb = new Database("Source") { ConnectionString = _source };
			var targetDb = new Database("Target") { ConnectionString = _target };

			Task.WaitAll(
				Task.Factory.StartNew(() => sourceDb.Load()),
				Task.Factory.StartNew(() => targetDb.Load()));

			DatabaseDiff diff = sourceDb.Compare(targetDb);
			if (diff.IsDiff) {
				Console.WriteLine("Databases are different.");
				return 1;
			}
			Console.WriteLine("Databases are identical.");
			return 0;
		}
	}
}