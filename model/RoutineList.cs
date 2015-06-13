using System.Collections;
using System.Collections.Generic;

namespace model
{
	public class RoutineList : IEnumerable<Routine> {

		private readonly Database _db;
		private readonly List<Routine> _list = new List<Routine>();

		public RoutineList(Database db) {
			_db = db;
		}

		public Routine Add(string schema, string name) {
			var r = new Routine(schema, name, _db);
			_list.Add(r);
			return r;
		}

		public void Clear() {
			_list.Clear();
		}

		public IEnumerator<Routine> GetEnumerator() {
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _list.GetEnumerator();
		}
	}
}
