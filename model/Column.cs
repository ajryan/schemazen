using System;

namespace model {
	public class Column : Attachable {
		public Default Default;
		public Identity Identity;
		public bool IsNullable;
		public int Length;
		public string Name;
		public int Position;
		public byte Precision;
		public int Scale;
		public string Type;

		public Column() {
		}

		public Column(string name, string type, bool @null, Default @default) {
			Name = name;
			Type = type;
			IsNullable = @null;
			Default = @default;
		}

		public Column(string name, string type, int length, bool @null, Default @default)
			: this(name, type, @null, @default) {
			Length = length;
		}

		public Column(string name, string type, byte precision, int scale, bool @null, Default @default)
			: this(name, type, @null, @default) {
			Precision = precision;
			Scale = scale;
		}

		private string IsNullableText => IsNullable ? "NULL" : "NOT NULL";

		public string DefaultText => Default == null
			? ""
			: "\r\n      " + Default.Script();

		public string IdentityText => Identity == null
			? ""
			: "\r\n      " + Identity.Script();

		public string TableName { get; set; }
		public string TableSchema { get; set; }

		public ColumnDiff Compare(Column c) {
			return new ColumnDiff(this, c);
		}


		public override void Attach() {
			Database.Tables.Find(TableName, TableSchema).Columns.Add(this);
		}

		public string Script() {
			switch (Type) {
				case "bigint":
				case "bit":
				case "date":
				case "datetime":
				case "datetime2":
				case "datetimeoffset":
				case "float":
				case "image":
				case "int":
				case "money":
				case "ntext":
				case "real":
				case "smalldatetime":
				case "smallint":
				case "smallmoney":
				case "sql_variant":
				case "text":
				case "time":
				case "timestamp":
				case "tinyint":
				case "uniqueidentifier":
				case "xml":
					return $"[{Name}] [{Type}] {IsNullableText} {DefaultText} {IdentityText}";

				case "binary":
				case "char":
				case "nchar":
				case "nvarchar":
				case "varbinary":
				case "varchar":
					string lengthString = Length == -1 ? "max" : Length.ToString();
					return $"[{Name}] [{Type}]({lengthString}) {IsNullableText} {DefaultText}";

				case "decimal":
				case "numeric":
					return $"[{Name}] [{Type}]({Precision},{Scale}) {IsNullableText} {DefaultText}";

				default:
					throw new NotSupportedException("SQL data type " + Type + " is not supported.");
			}
		}

		public void SetSize(object maxLength, object precision, object scale) {
			switch (Type)
			{
				case "binary":
				case "char":
				case "nchar":
				case "nvarchar":
				case "varbinary":
				case "varchar":
					Length = (int)maxLength;
					break;
				case "decimal":
				case "numeric":
					Precision = (byte)precision;
					Scale = (int)scale;
					break;
			}
		}
	}
}