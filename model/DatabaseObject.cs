namespace model
{
	public abstract class DatabaseObject {
		private Database _database;

		public Database Database {
			get { return _database; }
			set {
				_database = value;
				OnDatabaseChanged();
			}
		}

		protected virtual void OnDatabaseChanged() {}
	}
}
