namespace model {
	public abstract class AttachableCollection<T> : DatabaseObjectCollection<T> where T: Attachable {
		protected AttachableCollection(Database db) : base(db) {}
		protected AttachableCollection(DatabaseObject parent)  : base(parent) {}

		public void AttachAll() {
			foreach (var a in this) {
				a.Attach();
			};
		}
	}
}