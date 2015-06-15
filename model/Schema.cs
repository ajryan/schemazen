namespace model {
	public class Schema : Scriptable
	{
		public string Name;
		public string Owner;

		public Schema(string name, string owner)
		{
			Owner = owner;
			Name = name;
		}

		public override string BaseFileName => Name;

		public override string ScriptCreate() {
			string schemaName = Name.Replace("'", "''");
			string owner = Owner.Replace("'", "''");

			return string.Format(@"
				if not exists(select s.schema_id from sys.schemas s where s.name = '{0}')
					and exists(select p.principal_id from sys.database_principals p where p.name = '{1}') begin
					exec sp_executesql N'create schema [{0}] authorization [{1}]'
				end",
				schemaName,
				owner);
		}
	}
}