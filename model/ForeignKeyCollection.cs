using System.Linq;
using System.Threading.Tasks;

namespace model {
	public class ForeignKeyCollection : DatabaseObjectCollection<ForeignKey> {
		public ForeignKeyCollection(Database db) : base(db) { }
		public ForeignKeyCollection(DatabaseObject parent) : base(parent) { }

		public ForeignKey Find(string name) {
			return this.FirstOrDefault(fk => fk.Name == name);
		}

		public override Task LoadAsync() {
			return ExecuteQueryAsync(@"
					select
						tc.TABLE_SCHEMA,
						tc.TABLE_NAME,
						tc.CONSTRAINT_NAME,
						rc.UPDATE_RULE,
						rc.DELETE_RULE,
						fk.is_disabled
					from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
						inner join INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc on rc.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
						inner join sys.foreign_keys fk on rc.CONSTRAINT_NAME = fk.name
					where CONSTRAINT_TYPE = 'FOREIGN KEY'",
				dr =>
				{
					var fk = new ForeignKey(
						(string)dr["TABLE_NAME"],
						(string)dr["TABLE_SCHEMA"],
						(string)dr["CONSTRAINT_NAME"],
						(string)dr["UPDATE_RULE"],
						(string) dr["DELETE_RULE"],
						(bool) dr["is_disabled"]);
					Add(fk);
				});
		}
	}
}