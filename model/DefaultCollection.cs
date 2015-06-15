using System.Threading.Tasks;

namespace model
{
	public class DefaultCollection : AttachableCollection<Default> {
		public DefaultCollection(Database db) : base(db) {}

		public override Task LoadAsync() {
			return ExecuteQueryAsync(
				@"select
						s.name as TABLE_SCHEMA,
						t.name as TABLE_NAME,
						c.name as COLUMN_NAME,
						d.name as DEFAULT_NAME,
						d.definition as DEFAULT_VALUE
					from sys.tables t
						inner join sys.columns c on c.object_id = t.object_id
						inner join sys.default_constraints d on c.column_id = d.parent_column_id
							and d.parent_object_id = c.object_id
						inner join sys.schemas s on s.schema_id = t.schema_id",
				dr => {
					var colDef = new Default {
						TableSchema = (string) dr["TABLE_SCHEMA"],
						TableName = (string) dr["TABLE_NAME"],
						ColumnName = (string) dr["COLUMN_NAME"],
						Name = (string) dr["DEFAULT_NAME"],
						Value = (string) dr["DEFAULT_VALUE"]
					};
					Add(colDef);
				});
		}
	}
}
