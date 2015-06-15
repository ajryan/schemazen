using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace model
{
	public abstract class DatabaseObjectCollection<T> : IEnumerable<T>
		where T : DatabaseObject {
		private readonly List<T> _list = new List<T>();

		protected Database Database { get; set; }

		protected DatabaseObjectCollection(Database db) {
			Database = db;
		}

		public int Count => _list.Count;

		public void Add(T obj) {
			obj.Database = Database;
			_list.Add(obj);
		}

		public void Clear() {
			_list.Clear();
		}

		public abstract Task LoadAsync();

		public IEnumerator<T> GetEnumerator() {
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		protected Task ExecuteQueryAsync(string query, Action<SqlDataReader> readerAction) {
			return ExecuteQueryAsync(query, null, readerAction);
		}

		protected Task ExecuteQueryAsync(string query, Action<SqlCommand> cmdSetupAction, Action<SqlDataReader> readerAction) {
			return Task.Run(async () => {
				using (var cn = new SqlConnection(Database.ConnectionString)) {
					cn.Open();

					var cmd = cn.CreateCommand();
					cmd.CommandText = query;
					cmdSetupAction?.Invoke(cmd);

					var reader = await cmd.ExecuteReaderAsync();

					while (await reader.ReadAsync()) {
						readerAction(reader);
					}
				}
			});
		}
	}
}
