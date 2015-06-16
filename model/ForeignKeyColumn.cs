namespace model
{
	public class ForeignKeyColumn : Attachable {
		public string ConstraintName;
		public string RefTableName;
		public string RefTableSchema;
		public string ColumnName;
		public string RefColumnName;

		public override void Attach() {
			var fk = Database.ForeignKeys.Find(ConstraintName);

			fk.Columns.Add(ColumnName);
			fk.RefColumns.Add(RefColumnName);
			fk.RefTableName = RefTableName;
			fk.RefTableOwner = RefTableSchema;
		}
	}
}
