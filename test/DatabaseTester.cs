using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using model;
using NUnit.Framework;

namespace test {
	[TestFixture]
	public class DatabaseTester {
		public static void TestCopySchema(string pathToSchemaScript) {
			TestHelper.DropDb("TEST_SOURCE");
			TestHelper.DropDb("TEST_COPY");

			//create the db from sql script
			TestHelper.ExecSql("CREATE DATABASE TEST_SOURCE", "");
			for (int i = 1; i < 100; i++) {
				TestHelper.ExecSql(string.Format(@"
					CREATE TABLE [dbo].[MediaType{0}]
					(
							[MediaTypeId] INT NOT NULL,
							[Name] NVARCHAR(120),
							CONSTRAINT [PK_MediaType{0}] PRIMARY KEY CLUSTERED ([MediaTypeId])
					);", i), "TEST_SOURCE");

				TestHelper.ExecSql(string.Format("CREATE PROCEDURE dbo.Test{0} AS SELECT * FROM foo", i), "TEST_SOURCE");
			}
			TestHelper.ExecBatchSql(File.ReadAllText(pathToSchemaScript), "TEST_SOURCE");

			//load the model from newly created db and create a copy
			var copy = new Database("TEST_COPY");
			copy.ConnectionString = TestHelper.GetConnString("TEST_SOURCE");
			copy.Load();
			SqlConnection.ClearAllPools();
			TestHelper.ExecBatchSql(copy.ScriptCreate(), "master");

			//compare the dbs to make sure they are the same
			var source = new Database("TEST_SOURCE");
			source.ConnectionString = TestHelper.GetConnString("TEST_SOURCE");
			source.Load();
			copy.Load();
			TestCompare(source, copy);
		}

		private static void TestCompare(Database source, Database copy) {
			//compare the dbs to make sure they are the same
			Assert.IsFalse(source.Compare(copy).IsDiff);

			// get a second opinion
			// if you ever find your license key
			return;
			string cmd = string.Format("/c {0}\\SQLDBDiffConsole.exe {1} {2} {0}\\{3}",
				ConfigHelper.SqlDbDiffPath,
				"localhost\\SQLEXPRESS " + copy.Name + " NULL NULL Y",
				"localhost\\SQLEXPRESS " + source.Name + " NULL NULL Y",
				"SqlDbDiff.XML CompareResult.txt null");

			Console.WriteLine(cmd);
			var proc = new Process();
			proc.StartInfo.FileName = "cmd.exe";
			proc.StartInfo.Arguments = cmd;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			proc.Start();
			proc.WaitForExit();
			Assert.AreEqual("no difference", File.ReadAllLines("CompareResult.txt")[0]);
		}

		[Test]
		public void TestBug13() {
			TestCopySchema(Path.Combine(ConfigHelper.TestSchemaDir, "SANDBOX3_GBL.SQL"));
		}

		[Test]
		public void TestCollate() {
			string pathToSchema = Path.Combine(ConfigHelper.TestSchemaDir, "SANDBOX3_GBL.SQL");
			TestHelper.DropDb("TEST_SOURCE");

			//create the db from sql script
			TestHelper.ExecSql("CREATE DATABASE TEST_SOURCE", "");
			TestHelper.ExecBatchSql(File.ReadAllText(pathToSchema), "TEST_SOURCE");

			//load the model from newly created db and check collation
			var copy = new Database("TEST_COPY");
			copy.ConnectionString = TestHelper.GetConnString("TEST_SOURCE");
			copy.Load();

			Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", copy.DbProperties.Find("COLLATE").Value);
		}

		[Test]
		public void TestCopy() {
			foreach (string script in Directory.GetFiles(ConfigHelper.TestSchemaDir)) {
				Console.WriteLine("Testing {0}", script);
				TestCopySchema(script);
			}
		}

		[Test]
		[Ignore("test won't work without license key for sqldbdiff")]
		public void TestDiffScript() {
			TestHelper.DropDb("TEST_SOURCE");
			TestHelper.DropDb("TEST_COPY");

			//create the dbs from sql script
			string script = File.ReadAllText(ConfigHelper.TestSchemaDir + "\\BOP_QUOTE.sql");
			TestHelper.ExecSql("CREATE DATABASE TEST_SOURCE", "");
			TestHelper.ExecBatchSql(script, "TEST_SOURCE");

			script = File.ReadAllText(ConfigHelper.TestSchemaDir + "\\BOP_QUOTE_2.sql");
			TestHelper.ExecSql("CREATE DATABASE TEST_COPY", "");
			TestHelper.ExecBatchSql(script, "TEST_COPY");

			var source = new Database("TEST_SOURCE");
			source.ConnectionString = TestHelper.GetConnString("TEST_SOURCE");
			source.Load();

			var copy = new Database("TEST_COPY");
			copy.ConnectionString = TestHelper.GetConnString("TEST_COPY");
			copy.Load();

			//execute migration script to make SOURCE the same as COPY
			DatabaseDiff diff = copy.Compare(source);
			TestHelper.ExecBatchSql(diff.Script(), "TEST_SOURCE");

			//compare the dbs to make sure they are the same
			string cmd = string.Format("/c {0}\\SQLDBDiffConsole.exe {1} {2} {0}\\{3}", ConfigHelper.SqlDbDiffPath,
				"localhost\\SQLEXPRESS TEST_COPY   NULL NULL Y", "localhost\\SQLEXPRESS TEST_SOURCE NULL NULL Y",
				"SqlDbDiff.XML CompareResult.txt null");
			var proc = new Process();
			proc.StartInfo.FileName = "cmd.exe";
			proc.StartInfo.Arguments = cmd;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			proc.Start();
			proc.WaitForExit();

			Assert.AreEqual("no difference", File.ReadAllLines("CompareResult.txt")[0]);
		}

		[Test]
		public void TestFindTableRegEx() {
			var db = new Database();
			db.Tables.Add(new Table("dbo", "cmicDeductible"));
			db.Tables.Add(new Table("dbo", "cmicZipCode"));
			db.Tables.Add(new Table("dbo", "cmicState"));
			db.Tables.Add(new Table("dbo", "Policy"));
			db.Tables.Add(new Table("dbo", "Location"));
			db.Tables.Add(new Table("dbo", "Rate"));

			Assert.AreEqual(3, db.FindTablesRegEx("^cmic").Count);
			Assert.AreEqual(1, db.FindTablesRegEx("Location").Count);
		}

		[Test]
		public void TestScript() {
			var db = new Database("TEST_TEMP");
			var t1 = new Table("dbo", "t1");
			t1.Columns.Add(new Column("col1", "int", false, null));
			t1.Columns.Add(new Column("col2", "int", false, null));
			t1.Constraints.Add(new Constraint("pk_t1", "PRIMARY KEY", "col1,col2"));
			t1.Constraints.Find("pk_t1").Clustered = true;

			var t2 = new Table( "dbo", "t2");
			t2.Columns.Add(new Column("col1", "int", false, null));
			t2.Columns.Add(new Column("col2", "int", false, null));
			t2.Columns.Add(new Column("col3", "int", false, null));
			t2.Constraints.Add(new Constraint("pk_t2", "PRIMARY KEY", "col1"));
			t2.Constraints.Find("pk_t2").Clustered = true;
			t2.Constraints.Add(new Constraint("IX_col3", "UNIQUE", "col3"));

			db.ForeignKeys.Add(new ForeignKey(t2.Name, t2.Owner, "fk_t2_t1", "col2,col3", null, false, "col1,col2", t1.Name, t1.Owner));

			db.Tables.Add(t1);
			db.Tables.Add(t2);

			TestHelper.DropDb("TEST_TEMP");
			SqlConnection.ClearAllPools();
			TestHelper.ExecBatchSql(db.ScriptCreate(), "master");

			var db2 = new Database();
			db2.ConnectionString = TestHelper.GetConnString("TEST_TEMP");
			db2.Load();

			TestHelper.DropDb("TEST_TEMP");

			foreach (Table t in db.Tables) {
				Assert.IsNotNull(db2.Tables.Find(t.Name, t.Owner));
				Assert.IsFalse(db2.Tables.Find(t.Name, t.Owner).Compare(t).IsDiff);
			}
		}

		[Test]
		public void TestScriptDeletedProc() {
			var source = new Database();
			source.Routines.Add(new Routine("dbo", "test"));
			source.Routines.Find("test", "dbo").Type = "PROCEDURE";
			source.Routines.Find("test", "dbo").Text = @"
create procedure [dbo].[test]
as
select * from Table1
";

			var target = new Database();
			string scriptUp = target.Compare(source).Script();
			string scriptDown = source.Compare(target).Script();
			Assert.IsTrue(scriptUp.ToLower().Contains("drop procedure [dbo].[test]"));
			Assert.IsTrue(scriptDown.ToLower().Contains("create procedure [dbo].[test]"));
		}

		[Test]
		public void TestScriptToDir() {
			var db = new Database("ScriptToDirTest");

			var policy = new Table("dbo", "Policy");
			policy.Columns.Add(new Column("id", "int", false, null));
			policy.Columns.Add(new Column("form", "tinyint", false, null));
			policy.Constraints.Add(new Constraint("PK_Policy", "PRIMARY KEY", "id"));
			policy.Constraints[0].Clustered = true;
			policy.Constraints[0].Unique = true;
			policy.Columns[0].Identity = new Identity("dbo", "Policy", "id", "1", "1");

			var loc = new Table("dbo", "Location");
			loc.Columns.Add(new Column("id", "int", false, null));
			loc.Columns.Add(new Column("policyId", "int", false, null));
			loc.Columns.Add(new Column("storage", "bit", false, null));
			loc.Constraints.Add(new Constraint("PK_Location", "PRIMARY KEY", "id"));
			loc.Constraints[0].Clustered = true;
			loc.Constraints[0].Unique = true;
			loc.Columns[0].Identity = new Identity("dbo", "Location", "id", "1", "1");

			var formType = new Table("dbo", "FormType");
			formType.Columns.Add(new Column("code", "tinyint", false, null));
			formType.Columns.Add(new Column("desc", "varchar", 10, false, null));
			formType.Constraints.Add(new Constraint("PK_FormType", "PRIMARY KEY", "code"));
			formType.Constraints[0].Clustered = true;

			var fk_policy_formType = new ForeignKey(policy.Name, policy.Owner, "FK_Policy_FormType");
			fk_policy_formType.RefTableName = formType.Name;
			fk_policy_formType.RefTableOwner = formType.Owner;
			fk_policy_formType.Columns.Add("form");
			fk_policy_formType.RefColumns.Add("code");
			fk_policy_formType.OnUpdate = "NO ACTION";
			fk_policy_formType.OnDelete = "NO ACTION";

			var fk_location_policy = new ForeignKey(loc.Name, loc.Owner, "FK_Location_Policy");
			fk_location_policy.Columns.Add("policyId");
			fk_location_policy.RefTableName = policy.Name;
			fk_location_policy.RefTableOwner = policy.Owner;
			fk_location_policy.RefColumns.Add("id");
			fk_location_policy.OnUpdate = "NO ACTION";
			fk_location_policy.OnDelete = "CASCADE";

			db.Tables.Add(policy);
			db.Tables.Add(formType);
			db.Tables.Add(loc);
			db.ForeignKeys.Add(fk_policy_formType);
			db.ForeignKeys.Add(fk_location_policy);
			db.DbProperties.Find("COMPATIBILITY_LEVEL").Value = "90";
			db.DbProperties.Find("COLLATE").Value = "SQL_Latin1_General_CP1_CI_AS";
			db.DbProperties.Find("AUTO_CLOSE").Value = "OFF";
			db.DbProperties.Find("AUTO_SHRINK").Value = "ON";
			db.DbProperties.Find("ALLOW_SNAPSHOT_ISOLATION").Value = "ON";
			db.DbProperties.Find("READ_COMMITTED_SNAPSHOT").Value = "OFF";
			db.DbProperties.Find("RECOVERY").Value = "SIMPLE";
			db.DbProperties.Find("PAGE_VERIFY").Value = "CHECKSUM";
			db.DbProperties.Find("AUTO_CREATE_STATISTICS").Value = "ON";
			db.DbProperties.Find("AUTO_UPDATE_STATISTICS").Value = "ON";
			db.DbProperties.Find("AUTO_UPDATE_STATISTICS_ASYNC").Value = "ON";
			db.DbProperties.Find("ANSI_NULL_DEFAULT").Value = "ON";
			db.DbProperties.Find("ANSI_NULLS").Value = "ON";
			db.DbProperties.Find("ANSI_PADDING").Value = "ON";
			db.DbProperties.Find("ANSI_WARNINGS").Value = "ON";
			db.DbProperties.Find("ARITHABORT").Value = "ON";
			db.DbProperties.Find("CONCAT_NULL_YIELDS_NULL").Value = "ON";
			db.DbProperties.Find("NUMERIC_ROUNDABORT").Value = "ON";
			db.DbProperties.Find("QUOTED_IDENTIFIER").Value = "ON";
			db.DbProperties.Find("RECURSIVE_TRIGGERS").Value = "ON";
			db.DbProperties.Find("CURSOR_CLOSE_ON_COMMIT").Value = "ON";
			db.DbProperties.Find("CURSOR_DEFAULT").Value = "LOCAL";
			db.DbProperties.Find("TRUSTWORTHY").Value = "ON";
			db.DbProperties.Find("DB_CHAINING").Value = "ON";
			db.DbProperties.Find("PARAMETERIZATION").Value = "FORCED";
			db.DbProperties.Find("DATE_CORRELATION_OPTIMIZATION").Value = "ON";

			db.ConnectionString = ConfigHelper.TestDB.Replace("database=TESTDB", "database=" + db.Name);
			db.ExecCreate(true);

			DBHelper.ExecSql(db.ConnectionString,
				"  insert into formType ([code], [desc]) values (1, 'DP-1')\n"
				+ "insert into formType ([code], [desc]) values (2, 'DP-2')\n"
				+ "insert into formType ([code], [desc]) values (3, 'DP-3')");

			db.DataTables.Add(formType);
			db.Dir = db.Name;
			db.ScriptToDir();
			Assert.IsTrue(Directory.Exists(db.Name));
			Assert.IsTrue(Directory.Exists(db.Name + "\\data"));
			Assert.IsTrue(Directory.Exists(db.Name + "\\tables"));
			Assert.IsTrue(Directory.Exists(db.Name + "\\foreign_keys"));

			foreach (Table t in db.DataTables) {
				Assert.IsTrue(File.Exists(db.Name + "\\data\\" + t.Name));
			}
			foreach (Table t in db.Tables) {
				Assert.IsTrue(File.Exists(db.Name + "\\tables\\" + t.Name + ".sql"));
			}
			foreach (ForeignKey fk in db.ForeignKeys) {
				var expected = db.Name + "\\foreign_keys\\" + fk.TableName + ".sql";
				Assert.IsTrue(File.Exists(expected), "File does not exist" + expected);
			}

			var copy = new Database("ScriptToDirTestCopy");
			copy.Dir = db.Dir;
			copy.ConnectionString = ConfigHelper.TestDB.Replace("database=TESTDB", "database=" + copy.Name);
			copy.CreateFromDir(true);
			copy.Load();
			TestCompare(db, copy);
		}
	}
}