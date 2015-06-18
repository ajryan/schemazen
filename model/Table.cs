using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace model {
	public class Table : Scriptable {
		public string Name;
		public string Owner;

		public ColumnCollection Columns { get; }
		public ConstraintCollection Constraints { get; }

		public Table(string owner, string name) {
			Owner = owner;
			Name = name;

			Columns = new ColumnCollection(this);
			Constraints = new ConstraintCollection(this);
		}

		public override string BaseFileName => $"{Owner}.{Name}";

		public Constraint PrimaryKey => Constraints.FirstOrDefault(c => c.Type == "PRIMARY KEY");

		public TableDiff Compare(Table t) {
			var diff = new TableDiff {
				Owner = t.Owner,
				Name = t.Name
			};

			//get additions and compare mutual columns
			foreach (Column c in Columns) {
				Column c2 = t.Columns.Find(c.Name);
				if (c2 == null) {
					diff.ColumnsAdded.Add(c);
				}
				else {
					//compare mutual columns
					ColumnDiff cDiff = c.Compare(c2);
					if (cDiff.IsDiff) {
						diff.ColumnsDiff.Add(cDiff);
					}
				}
			}

			//get deletions
			foreach (Column c in t.Columns) {
				if (Columns.Find(c.Name) == null) {
					diff.ColumnsDroped.Add(c);
				}
			}

			//get added and compare mutual constraints
			foreach (Constraint c in Constraints) {
				Constraint c2 = t.Constraints.Find(c.Name);
				if (c2 == null) {
					diff.ConstraintsAdded.Add(c);
				}
				else {
					if (c.Script() != c2.Script()) {
						diff.ConstraintsChanged.Add(c);
					}
				}
			}
			//get deleted constraints
			foreach (Constraint c in t.Constraints) {
				if (Constraints.Find(c.Name) == null) {
					diff.ConstraintsDeleted.Add(c);
				}
			}

			return diff;
		}

		public override string ScriptCreate() {
			var text = new StringBuilder();
			text.AppendFormat("CREATE TABLE [{0}].[{1}](\r\n", Owner, Name);
			text.Append(Columns.ScriptAll());
			if (Constraints.Count > 0) text.AppendLine();
			foreach (Constraint c in Constraints) {
				if (c.Type == "INDEX") continue;
				text.AppendLine("   ," + c.Script());
			}
			text.AppendLine(")");
			text.AppendLine();
			foreach (Constraint c in Constraints) {
				if (c.Type != "INDEX") continue;
				text.AppendLine(c.Script());
			}
			return text.ToString();
		}

		public string ScriptDrop() {
			return string.Format("DROP TABLE [{0}].[{1}]", Owner, Name);
		}

		public string ExportData(string conn) {
			var data = new StringBuilder();
			var sql = new StringBuilder();
			sql.Append("select ");
			foreach (Column c in Columns) {
				sql.AppendFormat("[{0}],", c.Name);
			}
			sql.Remove(sql.Length - 1, 1);
			sql.AppendFormat(" from [{0}].[{1}]", Owner, Name);
			using (var cn = new SqlConnection(conn)) {
				cn.Open();
				using (SqlCommand cm = cn.CreateCommand()) {
					cm.CommandText = sql.ToString();
					using (SqlDataReader dr = cm.ExecuteReader()) {
						while (dr.Read()) {
							foreach (Column c in Columns) {
								data.AppendFormat("{0}\t", dr[c.Name]);
							}
							data.Remove(data.Length - 1, 1);
							data.AppendLine();
						}
					}
				}
			}

			return data.ToString();
		}

		public void ImportData(string conn, string data) {
			var dt = new DataTable();
			foreach (Column c in Columns) {
				dt.Columns.Add(new DataColumn(c.Name));
			}
			string[] lines = data.Split("\r\n".Split(','), StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < lines.Count(); i++) {
				string line = lines[i];
				DataRow row = dt.NewRow();
				string[] fields = line.Split('\t');
				if (fields.Length != Columns.Count) {
					throw new DataException("Incorrect number of columns", i + 1);
				}
				for (int j = 0; j < fields.Length; j++) {
					try {
						row[j] = ConvertType(Columns[j].Type, fields[j]);
					}
					catch (FormatException ex) {
						throw new DataException($"{ex.Message} at column {j + 1}", i + 1);
					}
				}
				dt.Rows.Add(row);
			}

			var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.TableLock) {
					DestinationTableName = Name
				};
			bulk.WriteToServer(dt);
		}

		public object ConvertType(string sqlType, string val) {
			if (val.Length == 0) return DBNull.Value;
			switch (sqlType.ToLower()) {
				case "bit":
					//added for compatibility with bcp
					if (val == "0") val = "False";
					if (val == "1") val = "True";
					return bool.Parse(val);
				case "datetime":
				case "smalldatetime":
					return DateTime.Parse(val);
				case "int":
					Int32.Parse(val);
					return val;
				default:
					return val;
			}
		}
	}
}