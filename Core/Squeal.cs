namespace ImageFunctions.Core;

// Return exceptions to be thrown
public static class Squeal
{
	public static Exception AlreadyRegistered(string @namespace, string name) {
		return new ArgumentException($"Item {@namespace}.{name} is already registered");
	}
	public static Exception ArgumentsMustBeEqual<T>(string name, T? v1, T? v2) where T : struct
	{
		string vals = (v1.HasValue && v2.HasValue) ? $" [{v1} : {v2}]" : "";
		var m = $"{name}(s) must be equal{vals}";
		return new ArgumentException(m);
	}
	public static Exception ArgumentNull(string argName) {
		throw new ArgumentNullException(argName);
	}
	public static Exception ArgumentNullOrEmpty(string argName) {
		throw new ArgumentException($"must not be null or empty",argName);
	}
	public static Exception ArgumentOutOfRange(string argName) {
		throw new ArgumentOutOfRangeException(argName);
	}
	public static Exception CouldNotLoadFile(string file, string extra) {
		throw new FileLoadException($"Could not load file ({extra})",file);
	}
	public static Exception EngineCannotDrawLines(string name) {
		return new NotSupportedException($"Engine {name} does not support drawing lines");
	}
	public static Exception FormatIsNotSupported(string name) {
		return new NotSupportedException($"format {name ?? "?"} is not supported");
	}
	public static Exception IndexOutOfRange(string argName) {
		throw new IndexOutOfRangeException(argName);
	}
	public static Exception InvalidArgument(string argName) {
		throw new ArgumentException("Invalid argument",argName);
	}
	public static Exception LayerMustHaveAtLeast(int count = 1) {
		var word = Tools.NumberToWord(count);
		var message = $"layers collection must contain at least {word} layer{(count > 1 ? "s" : "")}";
		return new ArgumentOutOfRangeException(message);
	}
	public static Exception NoLayers() {
		return new ArgumentOutOfRangeException($"No Layers are present");
	}
	public static Exception NotSupportedChannelCount(int channelCount) {
		throw new NotSupportedException($"Channel Count {channelCount} is not supported");
	}
	public static Exception NotSupportedCMYK() {
		return new NotSupportedException("CMYK is not supported");
	}
}