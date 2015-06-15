using System;

namespace model {
	public class Routine : Scriptable {
		public bool AnsiNull;
		public string Name;
		public bool QuotedId;
		public string Schema;
		public string Text;
		public string Type;

		public Routine(string schema, string name) {
			Schema = schema;
			Name = name;
		}

		public override string BaseFileName => $"{Schema}.{Name}";

		public void SetModuleType(string moduleType) {
			switch (moduleType)
			{
				case "SQL_STORED_PROCEDURE":
					Type = "PROCEDURE";
					break;
				case "SQL_TRIGGER":
					Type = "TRIGGER";
					break;
				case "SQL_SCALAR_FUNCTION":
					Type = "FUNCTION";
					break;
				case "VIEW":
					Type = "VIEW";
					break;
			}
		}

		public override string ScriptCreate() {
			var script = "";
			var defaultQuotedId = !QuotedId;
			if (Database != null && Database.DbProperties.Find("QUOTED_IDENTIFIER") != null) {
				defaultQuotedId = Database.DbProperties.Find("QUOTED_IDENTIFIER").Value == "ON";
			}
			if (defaultQuotedId != QuotedId) {
				script = string.Format(@"SET QUOTED_IDENTIFIER {0} {1}GO{1}",
					(QuotedId ? "ON" : "OFF"), Environment.NewLine);
			}
			var defaultAnsiNulls = !AnsiNull;
			if (Database != null && Database.DbProperties.Find("ANSI_NULLS") != null) {
				defaultAnsiNulls = Database.DbProperties.Find("ANSI_NULLS").Value == "ON";
			}
			if (defaultAnsiNulls != AnsiNull) {
				script = string.Format(@"SET ANSI_NULLS {0} {1}GO{1}",
					(AnsiNull ? "ON" : "OFF"), Environment.NewLine);
			}
			return script + Text;
		}

		public string ScriptDrop() {
			return $"DROP {Type} [{Schema}].[{Name}]";
		}
	}
}