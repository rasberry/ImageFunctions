namespace ImageFunctions
{
	public static class Tell
	{
		public static void CouldNotParse(string name, object val) {
			Log.Error($"invalid value '{val}' for '{name}'");
		}
		public static void UnknownAction(object val) {
			Log.Error($"unkown action '{val}'");
		}
		public static void ActionNotSpecified() {
			Log.Error("action was not specified");
		}
		public static void MustProvideInput(string name) {
			Log.Error($"option '{name}' is required");
		}
		public static void MissingArgument(string name) {
			Log.Error($"not enough arguments for '{name}'");
		}
		public static void MaxThreadsGreaterThanZero() {
			Log.Error("max-threads must be greater than zero");
		}
	}
}