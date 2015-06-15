using System;
using System.Linq;
using System.Threading.Tasks;

namespace model
{
	public class DbPropertyCollection : DatabaseObjectCollection<DbProperty> {
		public DbPropertyCollection(Database db) : base(db) {
			Add(new DbProperty("COMPATIBILITY_LEVEL", ""));
			Add(new DbProperty("COLLATE", ""));
			Add(new DbProperty("AUTO_CLOSE", ""));
			Add(new DbProperty("AUTO_SHRINK", ""));
			Add(new DbProperty("ALLOW_SNAPSHOT_ISOLATION", ""));
			Add(new DbProperty("READ_COMMITTED_SNAPSHOT", ""));
			Add(new DbProperty("RECOVERY", ""));
			Add(new DbProperty("PAGE_VERIFY", ""));
			Add(new DbProperty("AUTO_CREATE_STATISTICS", ""));
			Add(new DbProperty("AUTO_UPDATE_STATISTICS", ""));
			Add(new DbProperty("AUTO_UPDATE_STATISTICS_ASYNC", ""));
			Add(new DbProperty("ANSI_NULL_DEFAULT", ""));
			Add(new DbProperty("ANSI_NULLS", ""));
			Add(new DbProperty("ANSI_PADDING", ""));
			Add(new DbProperty("ANSI_WARNINGS", ""));
			Add(new DbProperty("ARITHABORT", ""));
			Add(new DbProperty("CONCAT_NULL_YIELDS_NULL", ""));
			Add(new DbProperty("NUMERIC_ROUNDABORT", ""));
			Add(new DbProperty("QUOTED_IDENTIFIER", ""));
			Add(new DbProperty("RECURSIVE_TRIGGERS", ""));
			Add(new DbProperty("CURSOR_CLOSE_ON_COMMIT", ""));
			Add(new DbProperty("CURSOR_DEFAULT", ""));
			Add(new DbProperty("TRUSTWORTHY", ""));
			Add(new DbProperty("DB_CHAINING", ""));
			Add(new DbProperty("PARAMETERIZATION", ""));
			Add(new DbProperty("DATE_CORRELATION_OPTIMIZATION", ""));
		}

		public override Task LoadAsync() {
			return ExecuteQueryAsync(
				@"select
					[compatibility_level],
					[collation_name],
					[is_auto_close_on],
					[is_auto_shrink_on],
					[snapshot_isolation_state],
					[is_read_committed_snapshot_on],
					[recovery_model_desc],
					[page_verify_option_desc],
					[is_auto_create_stats_on],
					[is_auto_update_stats_on],
					[is_auto_update_stats_async_on],
					[is_ansi_null_default_on],
					[is_ansi_nulls_on],
					[is_ansi_padding_on],
					[is_ansi_warnings_on],
					[is_arithabort_on],
					[is_concat_null_yields_null_on],
					[is_numeric_roundabort_on],
					[is_quoted_identifier_on],
					[is_recursive_triggers_on],
					[is_cursor_close_on_commit_on],
					[is_local_cursor_default],
					[is_trustworthy_on],
					[is_db_chaining_on],
					[is_parameterization_forced],
					[is_date_correlation_on]
				from sys.databases
				where name = @dbname",
				cm => cm.Parameters.AddWithValue("@dbname", Database.InitialCatalog),
				dr => {
					SetPropString("COMPATIBILITY_LEVEL", dr["compatibility_level"]);
					SetPropString("COLLATE", dr["collation_name"]);
					SetPropOnOff("AUTO_CLOSE", dr["is_auto_close_on"]);
					SetPropOnOff("AUTO_SHRINK", dr["is_auto_shrink_on"]);
					if (dr["snapshot_isolation_state"] != DBNull.Value) {
						SetPropOnOff("ALLOW_SNAPSHOT_ISOLATION",
							(byte) dr["snapshot_isolation_state"] != 0 &&
							(byte) dr["snapshot_isolation_state"] != 2);
					}
					SetPropOnOff("READ_COMMITTED_SNAPSHOT", dr["is_read_committed_snapshot_on"]);
					SetPropString("RECOVERY", dr["recovery_model_desc"]);
					SetPropString("PAGE_VERIFY", dr["page_verify_option_desc"]);
					SetPropOnOff("AUTO_CREATE_STATISTICS", dr["is_auto_create_stats_on"]);
					SetPropOnOff("AUTO_UPDATE_STATISTICS", dr["is_auto_update_stats_on"]);
					SetPropOnOff("AUTO_UPDATE_STATISTICS_ASYNC", dr["is_auto_update_stats_async_on"]);
					SetPropOnOff("ANSI_NULL_DEFAULT", dr["is_ansi_null_default_on"]);
					SetPropOnOff("ANSI_NULLS", dr["is_ansi_nulls_on"]);
					SetPropOnOff("ANSI_PADDING", dr["is_ansi_padding_on"]);
					SetPropOnOff("ANSI_WARNINGS", dr["is_ansi_warnings_on"]);
					SetPropOnOff("ARITHABORT", dr["is_arithabort_on"]);
					SetPropOnOff("CONCAT_NULL_YIELDS_NULL", dr["is_concat_null_yields_null_on"]);
					SetPropOnOff("NUMERIC_ROUNDABORT", dr["is_numeric_roundabort_on"]);
					SetPropOnOff("QUOTED_IDENTIFIER", dr["is_quoted_identifier_on"]);
					SetPropOnOff("RECURSIVE_TRIGGERS", dr["is_recursive_triggers_on"]);
					SetPropOnOff("CURSOR_CLOSE_ON_COMMIT", dr["is_cursor_close_on_commit_on"]);
					if (dr["is_local_cursor_default"] != DBNull.Value) {
						Find("CURSOR_DEFAULT").Value =
							(bool) dr["is_local_cursor_default"] ? "LOCAL" : "GLOBAL";
					}
					SetPropOnOff("TRUSTWORTHY", dr["is_trustworthy_on"]);
					SetPropOnOff("DB_CHAINING", dr["is_db_chaining_on"]);
					if (dr["is_parameterization_forced"] != DBNull.Value) {
						Find("PARAMETERIZATION").Value =
							(bool) dr["is_parameterization_forced"] ? "FORCED" : "SIMPLE";
					}
					SetPropOnOff("DATE_CORRELATION_OPTIMIZATION", dr["is_date_correlation_on"]);
				});
		}

		public DbProperty Find(string name) {
			return this.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		}

		public void SetPropOnOff(string propName, object dbVal) {
			if (dbVal != DBNull.Value) {
				Find(propName).Value = (bool)dbVal ? "ON" : "OFF";
			}
		}

		public void SetPropString(string propName, object dbVal) {
			if (dbVal != DBNull.Value) {
				Find(propName).Value = dbVal.ToString();
			}
		}
	}
}
