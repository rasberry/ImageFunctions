namespace ImageFunctions.Plugin;

// Return exceptions to be thrown
public static class PlugSqueal
{
	public static Exception NotImplementedSpace(AllColors.Space space) {
		throw new NotImplementedException($"Space {space} is not implemented");
	}
	public static Exception NotSupportedTypeByFunc(Type t, string funcName) {
		throw new NotSupportedException($"Type {t?.Name} is not supported by {funcName}");
	}
}