namespace model
{
	public interface IScriptable {
		string BaeFileName { get; }
		string ScriptCreate();
	}
}
