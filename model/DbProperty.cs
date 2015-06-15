namespace model {
	public class DbProperty : Scriptable {
		public DbProperty(string name, string value) {
			Name = name;
			Value = value;
		}

		public string Name { get; set; }
		public string Value { get; set; }

		public override string BaseFileName => Name;

		public override string ScriptCreate() {
			switch (Name.ToUpper()) {
				case "COLLATE":
					if (string.IsNullOrEmpty(Value)) return "";
					return $"EXEC('ALTER DATABASE [' + @DB + '] COLLATE {Value}')";

				case "COMPATIBILITY_LEVEL":
					if (string.IsNullOrEmpty(Value)) return "";
					return $"EXEC dbo.sp_dbcmptlevel @DB, {Value}";

				default:
					if (string.IsNullOrEmpty(Value)) return "";
					return $"EXEC('ALTER DATABASE [' + @DB + '] SET {Name} {Value}')";
			}
		}
	}
}