using System.Linq;
using System.Threading.Tasks;

namespace model {
	public class ForeignKeyCollection : DatabaseObjectCollection<ForeignKey> {
		public ForeignKeyCollection(Database db) : base(db) { }

		public ForeignKey Find(string name) {
			return this.FirstOrDefault(fk => fk.Name == name);
		}

		public override Task LoadAsync() {
			throw new System.NotImplementedException();
		}
	}
}