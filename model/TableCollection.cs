using System.Linq;
using System.Threading.Tasks;

namespace model {
	public class TableCollection : DatabaseObjectCollection<Table> {
		public TableCollection(Database db) : base(db) { }

		public Table Find(string name, string owner) {
			return this.FirstOrDefault(t => t.Name == name && t.Owner == owner);
		}

		public Constraint FindConstraint(string name) {
			return this.SelectMany(t => t.Constraints).FirstOrDefault(c => c.Name == name);
		}

		public override Task LoadAsync() {
			return ExecuteQueryAsync(
				@"select
					TABLE_SCHEMA,
					TABLE_NAME
					from INFORMATION_SCHEMA.TABLES
					where TABLE_TYPE = 'BASE TABLE'",
				dr => Add(new Table((string)dr["TABLE_SCHEMA"], (string)dr["TABLE_NAME"])));
		}
	}
}