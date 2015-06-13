using System;

namespace model {
	public class Routine : IScriptable {
		public bool AnsiNull;
		public string Name;
		public bool QuotedId;
		public string Schema;
		public string Text;
		public string Type;

		private Database _db;

		public Routine(string schema, string name, Database db) {
			Schema = schema;
			Name = name;
			_db = db;
		}

		public string BaeFileName {
			get { return string.Format("{0}.{1}", Schema, Name); }
		}

		public string ScriptCreate() {
			var script = "";
			var defaultQuotedId = !QuotedId;
			if (_db != null && _db.FindProp("QUOTED_IDENTIFIER") != null) {
				defaultQuotedId = _db.FindProp("QUOTED_IDENTIFIER").Value == "ON";
			}
			if (defaultQuotedId != QuotedId) {
				script = string.Format(@"SET QUOTED_IDENTIFIER {0} {1}GO{1}",
					(QuotedId ? "ON" : "OFF"), Environment.NewLine);
			}
			var defaultAnsiNulls = !AnsiNull;
			if (_db != null && _db.FindProp("ANSI_NULLS") != null) {
				defaultAnsiNulls = _db.FindProp("ANSI_NULLS").Value == "ON";
			}
			if (defaultAnsiNulls != AnsiNull) {
				script = string.Format(@"SET ANSI_NULLS {0} {1}GO{1}",
					(AnsiNull ? "ON" : "OFF"), Environment.NewLine);
			}
			return script + Text;
		}

		public string ScriptDrop() {
			return string.Format("DROP {0} [{1}].[{2}]", Type, Schema, Name);
		}
	}
}