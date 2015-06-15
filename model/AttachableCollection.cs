namespace model {
	public abstract class AttachableCollection<T> : DatabaseObjectCollection<T> where T: Attachable {
		protected AttachableCollection(Database db) : base(db) { }

		public void Attach() {
			foreach (var a in this) {
				a.Attach();
			};
		}
	}
}