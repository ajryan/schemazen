using model;
using NUnit.Framework;

namespace test {
	[TestFixture]
	public class ForeignKeyTester {
		public void TestMultiColumnKey() {
			var db = new Database("TESTDB");

			var t1 = new Table("dbo", "t1");
			t1.Columns.Add(new Column("c2", "varchar", 10, false, null));
			t1.Columns.Add(new Column("c1", "int", false, null));
			t1.Constraints.Add(new Constraint("pk_t1", "PRIMARY KEY", "c1,c2"));

			var t2 = new Table("dbo", "t2");
			t2.Columns.Add(new Column("c1", "int", false, null));
			t2.Columns.Add(new Column("c2", "varchar", 10, false, null));
			t2.Columns.Add(new Column("c3", "int", false, null));

			var fk = new ForeignKey(t2.Name, t2.Owner, "fk_test", "c3,c2", null, false, "c1,c2", t1.Name, t1.Owner);

			db.Tables.Add(t1);
			db.Tables.Add(t2);
			db.ForeignKeys.Add(fk);
			db.ConnectionString = TestHelper.GetConnString("TESTDB");
			db.ExecCreate(true);
			db.Load();

			Assert.AreEqual("c3", db.ForeignKeys.Find("fk_test").Columns[0]);
			Assert.AreEqual("c2", db.ForeignKeys.Find("fk_test").Columns[1]);
			Assert.AreEqual("c1", db.ForeignKeys.Find("fk_test").RefColumns[0]);
			Assert.AreEqual("c2", db.ForeignKeys.Find("fk_test").RefColumns[1]);

			db.ExecCreate(true);
		}

		[Test]
		public void TestScript() {
			var db = new Database();

			var person = new Table("dbo", "Person");
			person.Columns.Add(new Column("id", "int", false, null));
			person.Columns.Add(new Column("name", "varchar", 50, false, null));
			person.Columns.Find("id").Identity = new Identity("dbo", "Person", "id", "1", "1");	// TODO need Column.SetIdentity? or split the metadata from the dto (e.g. schema, table, column, etc
			person.Constraints.Add(new Constraint("PK_Person", "PRIMARY KEY", "id"));

			var address = new Table("dbo", "Address");
			address.Columns.Add(new Column("id", "int", false, null));
			address.Columns.Add(new Column("personId", "int", false, null));
			address.Columns.Add(new Column("street", "varchar", 50, false, null));
			address.Columns.Add(new Column("city", "varchar", 50, false, null));
			address.Columns.Add(new Column("state", "char", 2, false, null));
			address.Columns.Add(new Column("zip", "varchar", 5, false, null));
			address.Columns.Find("id").Identity = new Identity("dbo", "Address", "id", "1", "1");
			address.Constraints.Add(new Constraint("PK_Address", "PRIMARY KEY", "id"));

			var fk = new ForeignKey(address.Name, address.Owner, "FK_Address_Person", "", "CASCADE", false, "personId", person.Name, person.Owner, "id");

			TestHelper.ExecSql(person.ScriptCreate(), "");
			TestHelper.ExecSql(address.ScriptCreate(), "");
			TestHelper.ExecSql(fk.ScriptCreate(), "");
			TestHelper.ExecSql("drop table Address", "");
			TestHelper.ExecSql("drop table Person", "");
		}
	}
}