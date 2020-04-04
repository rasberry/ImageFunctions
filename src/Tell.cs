namespace ImageFunctions
{
	public static class Tell
	{
		public static void CouldNotParse(string name, object val) {
			Log.Error($"invalid value '{val}' for '{name}'");
		}
		public static void UnknownMethod(object val) {
			Log.Error($"unkown method '{val}'");
		}
		public static void MethodNotSpecified() {
			Log.Error("method was not specified");
		}
		public static void MustProvideInput(string name) {
			Log.Error($"option '{name}' is required");
		}
		public static void MissingArgument(string name) {
			Log.Error($"not enough arguments for '{name}'");
		}
	}
}