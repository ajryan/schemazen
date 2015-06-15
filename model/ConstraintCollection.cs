using System.Linq;
using System.Threading.Tasks;

namespace model {
	public class ConstraintCollection : AttachableCollection<Constraint> {
		public ConstraintCollection(Database database) : base(database) {}

		public override Task LoadAsync()
		{
			return ExecuteQueryAsync(
				@"select
					s.name as schemaName,
					t.name as tableName,
					i.name as indexName,
					c.name as columnName,
					i.is_primary_key,
					i.is_unique_constraint,
                    i.is_unique,
					i.type_desc,
                    isnull(ic.is_included_column, 0) as is_included_column
					from sys.tables t
						inner join sys.indexes i on i.object_id = t.object_id
						inner join sys.index_columns ic on ic.object_id = t.object_id
							and ic.index_id = i.index_id
						inner join sys.columns c on c.object_id = t.object_id
							and c.column_id = ic.column_id
						inner join sys.schemas s on s.schema_id = t.schema_id
					order by s.name, t.name, i.name, ic.key_ordinal, ic.index_column_id",
				dr => {
					string indexName = (string) dr["indexName"];

					var c = this.FirstOrDefault(i => i.Name == indexName)
						?? new Constraint(indexName, "INDEX");

					c.TableName = (string) dr["tableName"];
					c.SchemaName = (string) dr["schemaName"];
					c.Clustered = (string) dr["type_desc"] == "CLUSTERED";
					c.Unique = (bool) dr["is_unique"];

					if ((bool) dr["is_included_column"]) {
						c.IncludedColumns.Add((string) dr["columnName"]);
					}
					else {
						c.Columns.Add((string) dr["columnName"]);
					}

					if ((bool) dr["is_primary_key"]) c.Type = "PRIMARY KEY";
					if ((bool) dr["is_unique_constraint"]) c.Type = "UNIQUE";

					Add(c);
				});
		}
	}
}