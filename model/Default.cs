namespace model {
	public class Default : Attachable
	{
		public string TableSchema;
		public string TableName;
		public string ColumnName;

		public string Name;
		public string Value;

		public override void Attach() {
			Table t = Database.Tables.Find(TableName, TableSchema);
			t.Columns.Find(ColumnName).Default = this;
		}

		public string Script() {
			return $"CONSTRAINT [{Name}] DEFAULT {Value}";
		}
	}
}