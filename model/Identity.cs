namespace model {
	public class Identity : Attachable {
		public string TableSchema;
		public string TableName;
		public string ColumnName;
		public string Increment;
		public string Seed;

		public Identity(string tableSchema, string tableName, string columnName, string seed, string increment) {
			TableSchema = tableSchema;
			TableName = tableName;
			ColumnName = columnName;
			Seed = seed;
			Increment = increment;
		}

		public string Script() {
			return $"IDENTITY ({Seed},{Increment})";
		}

		public override void Attach() {
			Table t = Database.Tables.Find(TableName, TableSchema);
			Column c = t.Columns.Find(ColumnName);    // todo: inconsistent with t.FindXXX
			c.Identity = this;
		}
	}
}