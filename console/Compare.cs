using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FubuCore.CommandLine;
using model;

namespace console {
	public class CompareInput {
		[Description("Source database connection string.")]
		public string Source { get; set; }

		[Description("Target database connection string.")]
		public string Target { get; set; }
	}

	[CommandDescription("Compare databases", Name = "compare")]
	public class Compare : FubuCommand<CompareInput> {
		public override bool Execute(CompareInput input) {
			var sourceDb = new Database("Source") { ConnectionString = input.Source };
			var targetDb = new Database("Target") { ConnectionString = input.Target };

			Task.WaitAll(
				Task.Factory.StartNew(() => sourceDb.Load()),
				Task.Factory.StartNew(() => targetDb.Load()));

			DatabaseDiff diff = sourceDb.Compare(targetDb);

			Console.WriteLine($"Databases are {(diff.IsDiff ? "different" : "identical")}.");
			if (diff.IsDiff)
				Console.WriteLine(diff.DiffDescription);

            return diff.IsDiff;
		}
	}
}