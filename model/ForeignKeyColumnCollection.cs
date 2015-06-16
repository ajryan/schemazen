using System.Threading.Tasks;

namespace model {
	public class ForeignKeyColumnCollection : AttachableCollection<ForeignKeyColumn> {
		public ForeignKeyColumnCollection(Database db) : base(db) {}

		public override Task LoadAsync() {
			return ExecuteQueryAsync(@"
				select
					fk.name as CONSTRAINT_NAME,
					c1.name as COLUMN_NAME,
					OBJECT_SCHEMA_NAME(fk.referenced_object_id) as REF_TABLE_SCHEMA,
					OBJECT_NAME(fk.referenced_object_id) as REF_TABLE_NAME,
					c2.name as REF_COLUMN_NAME
					from sys.foreign_keys fk
					inner join sys.foreign_key_columns fkc
						on fkc.constraint_object_id = fk.object_id
					inner join sys.columns c1
						on fkc.parent_column_id = c1.column_id
						and fkc.parent_object_id = c1.object_id
					inner join sys.columns c2
						on fkc.referenced_column_id = c2.column_id
						and fkc.referenced_object_id = c2.object_id
					order by fk.name",
				dr => {
					var fkc = new ForeignKeyColumn {
						ConstraintName = (string) dr["CONSTRAINT_NAME"],
						ColumnName = (string) dr["COLUMN_NAME"],
						RefColumnName = (string) dr["REF_COLUMN_NAME"],
						RefTableName = (string)dr["REF_TABLE_NAME"],
						RefTableSchema = (string)dr["REF_TABLE_SCHEMA"]
					};
					Add(fkc);
				});
		}
	}
}