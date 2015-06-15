using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace model {
	public class Database {
		#region " Constructors "

		public Database() {
			DbProperties = new DbPropertyCollection(this);
			Schemas = new SchemaCollection(this);
			Tables = new TableCollection(this);
			ForeignKeys = new ForeignKeyCollection(this);
			Routines = new RoutineCollection(this);
			DataTables = new TableCollection(this);
		}

		public Database(string name) : this() {
			Name = name;
		}

		#endregion

		#region " Properties "

		private string _connectionString = "";

		public string ConnectionString {
			get { return _connectionString; }
			set {
				var cnStrBuilder = new SqlConnectionStringBuilder(value) {
					AsynchronousProcessing = true
				};
				_connectionString = cnStrBuilder.ConnectionString;
			}
		}

		public string Dir = "";
		public string Name;

		public DbPropertyCollection DbProperties { get; }
		public SchemaCollection Schemas { get; }
		public TableCollection Tables { get; }
		public ForeignKeyCollection ForeignKeys { get; }
		public RoutineCollection Routines { get; }
		public TableCollection DataTables { get;  }

		public string InitialCatalog => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

		public List<Table> FindTablesRegEx(string pattern) {
			return Tables.Where(t => Regex.IsMatch(t.Name, pattern)).ToList();
		}

		#endregion

		private static readonly string[] SubDirs = {"tables", "foreign_keys", "functions", "procs", "triggers", "views"};

		private string DataDir => Path.Combine(Dir, "data");

		public void Load() {
			Tables.Clear();
			Routines.Clear();
			ForeignKeys.Clear();
			DataTables.Clear();

			// query schema for database properties
			var columns = new ColumnCollection(this);
			var identities = new IdentityCollection(this);
			var defaults = new DefaultCollection(this);
			var indexes = new ConstraintCollection(this);

			Task.WaitAll(
				DbProperties.LoadAsync(),
				Schemas.LoadAsync(),
				Tables.LoadAsync(),
				columns.LoadAsync(),
				identities.LoadAsync(),
				defaults.LoadAsync(),
				indexes.LoadAsync());

			columns.Attach();
			identities.Attach();
			defaults.Attach();
			indexes.Attach();

			//		//get foreign keys
			//var fkEvent = ExecuteQueryAsync(
			//	@"select
			//			TABLE_SCHEMA,
			//			TABLE_NAME,
			//			CONSTRAINT_NAME
			//		from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
			//		where CONSTRAINT_TYPE = 'FOREIGN KEY'",
			//	dr => {
			//		Table t = Tables.Find((string) dr["TABLE_NAME"], (string) dr["TABLE_SCHEMA"]);
			//		var fk = new ForeignKey((string) dr["CONSTRAINT_NAME"]) { Table = t };
			//		ForeignKeys.Add(fk);
			//	});

			//		//get foreign key props
			//var fkPropsEvent = ExecuteQueryAsync(
			//	@"select
			//		CONSTRAINT_NAME,
			//		UPDATE_RULE,
			//		DELETE_RULE,
			//		fk.is_disabled
			//	from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
			//		inner join sys.foreign_keys fk on rc.CONSTRAINT_NAME = fk.name",
			//	dr => {
			//		ForeignKey fk = ForeignKeys.Find((string) dr["CONSTRAINT_NAME"]);
			//		fk.OnUpdate = (string) dr["UPDATE_RULE"];
			//		fk.OnDelete = (string) dr["DELETE_RULE"];
			//		fk.Check = !(bool) dr["is_disabled"];
			//	});

			//		//get foreign key columns and ref table
			//var fkRefEvent = ExecuteQueryAsync(
			//	@"select
			//	fk.name as CONSTRAINT_NAME,
			//	c1.name as COLUMN_NAME,
			//	OBJECT_SCHEMA_NAME(fk.referenced_object_id) as REF_TABLE_SCHEMA,
			//	OBJECT_NAME(fk.referenced_object_id) as REF_TABLE_NAME,
			//	c2.name as REF_COLUMN_NAME
			//from sys.foreign_keys fk
			//inner join sys.foreign_key_columns fkc
			//	on fkc.constraint_object_id = fk.object_id
			//inner join sys.columns c1
			//	on fkc.parent_column_id = c1.column_id
			//	and fkc.parent_object_id = c1.object_id
			//inner join sys.columns c2
			//	on fkc.referenced_column_id = c2.column_id
			//	and fkc.referenced_object_id = c2.object_id
			//order by fk.name",
			//	dr => {
			//		ForeignKey fk = ForeignKeys.Find((string) dr["CONSTRAINT_NAME"]);
			//		if (fk == null) {
			//			return;
			//		}
			//		fk.Columns.Add((string) dr["COLUMN_NAME"]);
			//		fk.RefColumns.Add((string) dr["REF_COLUMN_NAME"]);
			//		if (fk.RefTable == null) {
			//			fk.RefTable = Tables.Find((string) dr["REF_TABLE_NAME"], (string) dr["REF_TABLE_SCHEMA"]);
			//		}
			//	});

			//		//get routines
			//var routinesEvent = ExecuteQueryAsync(
			//	@"select
			//			s.name as schemaName,
			//			o.name as routineName,
			//			o.type_desc,
			//			m.definition,
			//                     m.uses_ansi_nulls,
			//			m.uses_quoted_identifier,
			//			t.name as tableName
			//		from sys.sql_modules m
			//			inner join sys.objects o on m.object_id = o.object_id
			//			inner join sys.schemas s on s.schema_id = o.schema_id
			//			left join sys.triggers tr on m.object_id = tr.object_id
			//			left join sys.tables t on tr.parent_id = t.object_id",
			//	dr => {
			//		var r = new Routine((string) dr["schemaName"], (string) dr["routineName"]) {
			//			Text = (string) dr["definition"],
			//			AnsiNull = (bool) dr["uses_ansi_nulls"],
			//			QuotedId = (bool) dr["uses_quoted_identifier"]
			//		};

			//		r.SetModuleType((string)dr["type_desc"]);

			//		Routines.Add(r);
			//	});
		}

		public DatabaseDiff Compare(Database db) {
			var diff = new DatabaseDiff { Db = db };

			//compare database properties
			foreach (DbProperty p in DbProperties) {
				DbProperty p2 = db.DbProperties.Find(p.Name);
				if (p.ScriptCreate() != p2.ScriptCreate()) {
					diff.PropsChanged.Add(p);
				}
			}

			//get tables added and changed
			foreach (Table t in Tables) {
				Table t2 = db.Tables.Find(t.Name, t.Owner);
				if (t2 == null) {
					diff.TablesAdded.Add(t);
				}
				else {
					//compare mutual tables
					TableDiff tDiff = t.Compare(t2);
					if (tDiff.IsDiff) {
						diff.TablesDiff.Add(tDiff);
					}
				}
			}
			//get deleted tables
			foreach (Table t in db.Tables) {
				if (Tables.Find(t.Name, t.Owner) == null) {
					diff.TablesDeleted.Add(t);
				}
			}

			//get procs added and changed
			foreach (Routine r in Routines) {
				Routine r2 = db.Routines.Find(r.Name, r.Schema);
				if (r2 == null) {
					diff.RoutinesAdded.Add(r);
				}
				else {
					//compare mutual procs
					if (r.Text != r2.Text) {
						diff.RoutinesDiff.Add(r);
					}
				}
			}
			//get procs deleted
			foreach (Routine r in db.Routines) {
				if (Routines.Find(r.Name, r.Schema) == null) {
					diff.RoutinesDeleted.Add(r);
				}
			}

			//get added and compare mutual foreign keys
			foreach (ForeignKey fk in ForeignKeys) {
				ForeignKey fk2 = db.ForeignKeys.Find(fk.Name);
				if (fk2 == null) {
					diff.ForeignKeysAdded.Add(fk);
				}
				else {
					if (fk.ScriptCreate() != fk2.ScriptCreate()) {
						diff.ForeignKeysDiff.Add(fk);
					}
				}
			}
			//get deleted foreign keys
			foreach (ForeignKey fk in db.ForeignKeys) {
				if (ForeignKeys.Find(fk.Name) == null) {
					diff.ForeignKeysDeleted.Add(fk);
				}
			}

			return diff;
		}

		public string ScriptCreate() {
			var text = new StringBuilder();

			text.AppendFormat("CREATE DATABASE {0}", Name);
			text.AppendLine();
			text.AppendLine("GO");
			text.AppendFormat("USE {0}", Name);
			text.AppendLine();
			text.AppendLine("GO");
			text.AppendLine();

			if (DbProperties.Count > 0) {
				text.Append(ScriptProperties(DbProperties));
				text.AppendLine("GO");
				text.AppendLine();
			}

			if (Schemas.Count > 0) {
				text.Append(ScriptSchemas(Schemas));
				text.AppendLine("GO");
				text.AppendLine();
			}

			foreach (Table t in Tables) {
				text.AppendLine(t.ScriptCreate());
			}
			text.AppendLine();
			text.AppendLine("GO");

			foreach (ForeignKey fk in ForeignKeys) {
				text.AppendLine(fk.ScriptCreate());
			}
			text.AppendLine();
			text.AppendLine("GO");

			foreach (Routine r in Routines) {
				text.AppendLine(r.ScriptCreate());
				text.AppendLine();
				text.AppendLine("GO");
			}

			return text.ToString();
		}

		public void ScriptToDir(bool overwrite) {
			Directory.CreateDirectory(Dir);

			// delete existing directory tree
			foreach (string d in SubDirs.Select(subDir => Path.Combine(Dir, subDir))) {
				if (Directory.Exists(d)) {
					Directory.Delete(d, true);
				}
			}

			var text = new StringBuilder();
			text.Append(ScriptProperties(DbProperties));
			text.AppendLine("GO");
			text.AppendLine();
			File.WriteAllText($"{Dir}/props.sql",
				text.ToString());

			if (Schemas.Count > 0) {
				text = new StringBuilder();
				text.Append(ScriptSchemas(Schemas));
				text.AppendLine("GO");
				text.AppendLine();
				File.WriteAllText($"{Dir}/schemas.sql",
					text.ToString());
			}

			ScriptToSubdir(Tables, "tables");

			ScriptToSubdir(ForeignKeys, "foreign_keys", true);

			ScriptToSubdir(Routines.Where(r => r.Type == "PROCEDURE"), "procs");
			ScriptToSubdir(Routines.Where(r => r.Type == "TRIGGER"), "triggers");
			ScriptToSubdir(Routines.Where(r => r.Type == "FUNCTION"), "functions");
			ScriptToSubdir(Routines.Where(r => r.Type == "VIEW"), "views");

			ExportData();
		}

		private void ScriptToSubdir(IEnumerable<Scriptable> scriptables, string subDir, bool append = false) {
			var scriptableList = scriptables as List<Scriptable> ?? scriptables.ToList();

			if (!scriptableList.Any()) return;

			string targetDir = Path.Combine(Dir, subDir);
			Directory.CreateDirectory(targetDir);

			foreach (var scriptable in scriptableList) {
				string scriptFile = Path.Combine(targetDir, MakeFileName(scriptable));
				string scriptContents = scriptable.ScriptCreate() + "\r\nGO\r\n";

				if (append) {
					File.AppendAllText(scriptFile, scriptContents);
				}
				else {
					File.WriteAllText(scriptFile, scriptContents);
				}
			}
		}

		private static string MakeFileName(Scriptable s) {
			// Dont' include schema name for objects in the dbo schema.
			// This maintains backward compatability for those who use
			// schemazen to keep their schemas under version control.
			string baseName = s.BaseFileName.StartsWith("dbo.")
				? s.BaseFileName.Substring(4)
				: s.BaseFileName;
			return baseName + ".sql";
		}

		public void ExportData() {
			if (DataTables.Count == 0) return;

			Directory.CreateDirectory(DataDir);

			foreach (Table t in DataTables) {
				File.WriteAllText(Path.Combine(DataDir, MakeFileName(t)), t.ExportData(ConnectionString));
			}
		}

		public void ImportData() {
			if (!Directory.Exists(DataDir)) {
				return;
			}

			foreach (var f in Directory.GetFiles(DataDir)) {
				var fi = new FileInfo(f);
				var schema = "dbo";
				var table = fi.Name;
				if (fi.Name.Contains(".")) {
					schema = fi.Name.Split('.')[0];
					table = fi.Name.Split('.')[1];
				}
				var t = Tables.Find(table, schema);
				if (t == null) {
					continue;
				}
				try {
					t.ImportData(ConnectionString, File.ReadAllText(Path.Combine(DataDir, MakeFileName(t))));
				}
				catch (DataException ex) {
					throw new DataFileException(ex.Message, fi.FullName, ex.LineNumber);
				}
			}
		}

		public void CreateFromDir(bool overwrite) {
			if (DBHelper.DbExists(ConnectionString)) {
				DBHelper.DropDb(ConnectionString);
			}

			//create database
			DBHelper.CreateDb(ConnectionString);

			//run scripts
			string propsPath = Path.Combine(Dir, "props.sql");
			if (File.Exists(propsPath)) {
				try {
					DBHelper.ExecBatchSql(ConnectionString, File.ReadAllText(propsPath));
				}
				catch (SqlBatchException ex) {
					throw new SqlFileException(propsPath, ex);
				}

				// COLLATE can cause connection to be reset
				// so clear the pool so we get a new connection
				DBHelper.ClearPool(ConnectionString);
			}

			string schemaPath = Path.Combine(Dir, "schemas.sql");
			if (File.Exists(schemaPath))
			{
				try
				{
					DBHelper.ExecBatchSql(ConnectionString, File.ReadAllText(schemaPath));
				}
				catch (SqlBatchException ex)
				{
					throw new SqlFileException(schemaPath, ex);
				}
			}

			// create db objects
			// resolve dependencies by trying over and over
			// if the number of failures stops decreasing then give up
			List<string> scripts = GetScripts();
			var errors = new List<SqlFileException>();
			int prevCount = Int32.MaxValue;
			while (scripts.Count > 0 && errors.Count < prevCount) {
				if (errors.Count > 0) {
					prevCount = errors.Count;
					Console.WriteLine(
						"{0} errors occurred, retrying...", errors.Count);
				}
				errors.Clear();
				foreach (string f in scripts.ToArray()) {
					try {
						DBHelper.ExecBatchSql(ConnectionString, File.ReadAllText(f));
						scripts.Remove(f);
					}
					catch (SqlBatchException ex) {
						errors.Add(new SqlFileException(f, ex));
					}
				}
			}

			Load(); // load the schema first so we can import data
			ImportData(); // load data

			// foreign keys
			string fkPath = Path.Combine(Dir, "foreign_keys");
			if (Directory.Exists(fkPath)) {
				foreach (string f in Directory.GetFiles(fkPath, "*.sql")) {
					try {
						DBHelper.ExecBatchSql(ConnectionString, File.ReadAllText(f));
					}
					catch (SqlBatchException ex) {
						throw new SqlFileException(f, ex);
					}
				}
			}
			if (errors.Count > 0) {
				var ex = new BatchSqlFileException { Exceptions = errors };
				throw ex;
			}
		}

		private List<string> GetScripts() {
			var scripts = new List<string>();
			foreach (string subDir in SubDirs) {
				if ("foreign_keys" == subDir) {
					continue;
				}
				string subDirPath = Path.Combine(Dir, subDir);
				if (!Directory.Exists(subDirPath)) {
					continue;
				}
				scripts.AddRange(Directory.GetFiles(subDirPath, "*.sql"));
			}
			return scripts;
		}

		public void ExecCreate(bool dropIfExists) {
			var conStr = new SqlConnectionStringBuilder(ConnectionString);
			string dbName = conStr.InitialCatalog;
			conStr.InitialCatalog = "master";
			if (DBHelper.DbExists(ConnectionString)) {
				if (dropIfExists) {
					DBHelper.DropDb(ConnectionString);
				}
				else {
					throw new ApplicationException($"Database {conStr.DataSource} {dbName} already exists.");
				}
			}
			DBHelper.ExecBatchSql(conStr.ToString(), ScriptCreate());
		}

		public static string ScriptProperties(IEnumerable<DbProperty> properties) {
			var text = new StringBuilder();

			text.AppendLine("DECLARE @DB VARCHAR(255)");
			text.AppendLine("SET @DB = DB_NAME()");

			foreach (DbProperty p in properties) {
				text.AppendLine(p.ScriptCreate());
			}
			return text.ToString();
		}

		public static string ScriptSchemas(IEnumerable<Schema> schemas)
		{
			var text = new StringBuilder();
			foreach (Schema s in schemas) {
				text.Append(s.ScriptCreate());
			}
			return text.ToString();
		}
	}
}