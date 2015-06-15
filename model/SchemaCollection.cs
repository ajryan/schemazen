using System.Threading.Tasks;

namespace model {
	public class SchemaCollection : DatabaseObjectCollection<Schema> {
		public SchemaCollection(Database db) : base(db) { }

		public override Task LoadAsync() {
			return ExecuteQueryAsync(
				@"select s.name as schemaName, p.name as principalName
					from sys.schemas s
					inner join sys.database_principals p on s.principal_id = p.principal_id
					where s.schema_id < 16384
					and s.name not in ('dbo','guest','sys','INFORMATION_SCHEMA')
					order by schema_id",
				dr => Add(new Schema((string)dr["schemaName"], (string)dr["principalName"])));
		}
	}
}