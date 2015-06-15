using System.Threading.Tasks;

namespace model {
	public class IdentityCollection : AttachableCollection<Identity> {
		public IdentityCollection(Database db) : base(db) { }

		public override Task LoadAsync() {
			return ExecuteQueryAsync(
				@"select
					s.name as TABLE_SCHEMA,
					t.name as TABLE_NAME,
					c.name AS COLUMN_NAME,
					i.SEED_VALUE, i.INCREMENT_VALUE
					from sys.tables t
						inner join sys.columns c on c.object_id = t.object_id
						inner join sys.identity_columns i on i.object_id = c.object_id
							and i.column_id = c.column_id
						inner join sys.schemas s on s.schema_id = t.schema_id",
				dr => {
					string tableSchema = (string) dr["TABLE_SCHEMA"];
                    string tableName = (string)dr["TABLE_NAME"];
					string columnName = (string)dr["COLUMN_NAME"];
					string seed = dr["SEED_VALUE"].ToString();
					string increment = dr["INCREMENT_VALUE"].ToString();

					Add(new Identity(tableSchema, tableName, columnName, seed, increment));
				});
		}
	}
}