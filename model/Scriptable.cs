namespace model {
	public abstract class Scriptable : DatabaseObject {
		public abstract string BaseFileName { get; }
		public abstract string ScriptCreate();
	}
}