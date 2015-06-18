using System;
using FubuCore.CommandLine;

namespace console {
	internal class Program {

		private static int Main(string[] args) {
			try {
				var factory = new CommandFactory();
				factory.SetAppName("schemazen");
				factory.RegisterCommands(typeof (Program).Assembly);

				var executor = new CommandExecutor(factory);
				return executor.Execute(args) ? 0 : 1;
			}
			catch (Exception ex) {
				var commandEx = ex as CommandFailureException;

				Console.ForegroundColor = ConsoleColor.Red;
				ConsoleWriter.Write("ERROR: " + (commandEx?.Message ?? ex.ToString()));
				Console.ResetColor();
				return 1;
			}
		}
	}
}