using System.Linq;
using System.Threading.Tasks;

namespace model
{
	public class RoutineCollection : DatabaseObjectCollection<Routine> {
		public RoutineCollection(Database db) : base(db) {}

		public Routine Find(string name, string schema) {
			return this.FirstOrDefault(r => r.Name == name && r.Schema == schema);
		}

		public override Task LoadAsync() {
			return ExecuteQueryAsync(@"
				select
					s.name as schemaName,
					o.name as routineName,
					o.type_desc,
					m.definition,
					m.uses_ansi_nulls,
					m.uses_quoted_identifier,
					t.name as tableName
				from sys.sql_modules m
					inner join sys.objects o on m.object_id = o.object_id
					inner join sys.schemas s on s.schema_id = o.schema_id
					left join sys.triggers tr on m.object_id = tr.object_id
					left join sys.tables t on tr.parent_id = t.object_id",
				dr =>
				{
					var r = new Routine((string)dr["schemaName"], (string)dr["routineName"])
					{
						Text = (string)dr["definition"],
						AnsiNull = (bool)dr["uses_ansi_nulls"],
						QuotedId = (bool)dr["uses_quoted_identifier"]
					};

					r.SetModuleType((string)dr["type_desc"]);

					Add(r);
				});
		}
	}
}
