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
			throw new System.NotImplementedException();
		}
	}
}
