using System;
using System.Linq;
using System.Threading.Tasks;

namespace model
{
	public class ColumnCollection : AttachableCollection<Column> {
		public ColumnCollection(Database db) : base(db) {}
		public ColumnCollection(DatabaseObject parent)  : base(parent) { }

		public Column Find(string name) {
			return this.FirstOrDefault(c => c.Name == name);
		}

		public string ScriptAll() {
			return string.Join($",{Environment.NewLine}", this.Select(c => c.Script()));
		}

		public override Task LoadAsync() {
			return ExecuteQueryAsync(
				@"select
					t.TABLE_SCHEMA,
					c.TABLE_NAME,
					c.COLUMN_NAME,
					c.DATA_TYPE,
					c.IS_NULLABLE,
					c.CHARACTER_MAXIMUM_LENGTH,
					c.NUMERIC_PRECISION,
					c.NUMERIC_SCALE
					from INFORMATION_SCHEMA.COLUMNS c
					inner join INFORMATION_SCHEMA.TABLES t
						on t.TABLE_NAME = c.TABLE_NAME
							and t.TABLE_SCHEMA = c.TABLE_SCHEMA
							and t.TABLE_CATALOG = c.TABLE_CATALOG
					where
						t.TABLE_TYPE = 'BASE TABLE'",
				dr => {
					var c = new Column
					{
						TableName = (string)dr["TABLE_NAME"],
						TableSchema = (string)dr["TABLE_SCHEMA"],
						Name = (string)dr["COLUMN_NAME"],
						Type = (string)dr["DATA_TYPE"],
						IsNullable = (string)dr["IS_NULLABLE"] == "YES"
					};

					c.SetSize(
						dr["CHARACTER_MAXIMUM_LENGTH"],
						dr["NUMERIC_PRECISION"],
						dr["NUMERIC_SCALE"]);

					Add(c);
				});
		}
	}
}
