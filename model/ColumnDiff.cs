namespace model {
	public class ColumnDiff {
		public Column Source;
		public Column Target;

		public ColumnDiff(Column target, Column source) {
			Source = source;
			Target = target;
		}

		public bool IsDiff {
			get {
				return
					Source.DefaultText != Target.DefaultText ||
					Source.IsNullable != Target.IsNullable ||
					Source.Length != Target.Length ||
					Source.Position != Target.Position ||
					Source.Type != Target.Type ||
					Source.Precision != Target.Precision ||
					Source.Scale != Target.Scale;
			}
		}

		public string Script() {
			return Target.Script();
		}
	}
}