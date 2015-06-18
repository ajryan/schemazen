using System;
using System.ComponentModel;
using System.Data.SqlClient;
using FubuCore.CommandLine;
using model;

namespace console {
	public class DbCommandInput {
		public string Server { get; set; }
		public string Database { get; set; }

		[Description("Path to database script directory.")]
		public string ScriptDir { get; set; }

		[FlagAlias("user", 'u')]
		public string UserFlag { get; set; }

		[FlagAlias("password", 'p')]
		public string PasswordFlag { get; set; }

		[Description("Overwrite the target script directory without prompt.")]
		[FlagAlias("overwrite", 'o')]
		public bool OverwriteFlag { get; set; }

		public Database CreateDatabase()
		{
			bool integratedSecurity = string.IsNullOrEmpty(UserFlag);
			var builder = new SqlConnectionStringBuilder
			{
				DataSource = Server,
				InitialCatalog = Database,
				IntegratedSecurity = integratedSecurity
			};
			if (!integratedSecurity)
			{
				builder.UserID = UserFlag;
				builder.Password = PasswordFlag;
			}
			return new Database
			{
				ConnectionString = builder.ToString(),
				Dir = ScriptDir
			};
		}
	}
}
