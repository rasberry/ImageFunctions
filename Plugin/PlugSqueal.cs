namespace ImageFunctions.Plugin;

// Return exceptions to be thrown
public static class PlugSqueal
{
	public static Exception NotImplementedSpace(Functions.AllColors.Space space) {
		throw new NotImplementedException($"Space {space} is not implemented");
	}
	public static Exception NotSupportedTypeByFunc(Type t, string funcName) {
		throw new NotSupportedException($"Type {t?.Name} is not supported by {funcName}");
	}
	public static Exception OutOfRange(string name, string message = null) {
		return String.IsNullOrWhiteSpace(message)
			? new ArgumentOutOfRangeException(name)
			: new ArgumentOutOfRangeException(name, message);
	}
}