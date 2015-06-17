using System;
using ManyConsole;

namespace console {
	internal class Program {

		private static int Main(string[] args) {
			try {
				return ConsoleCommandDispatcher.DispatchCommand(
				  new ConsoleCommand[] {
					  new Script(), new Create(), new Compare()
				  },
				  args,
				  Console.Out);
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				return -1;
			}
		}
	}
}