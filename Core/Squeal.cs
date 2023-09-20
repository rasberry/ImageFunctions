namespace ImageFunctions.Core;

// Return exceptions to be thrown
public static class Squeal
{
	public static Exception FormatIsNotSupported(string name) {
		return new NotSupportedException($"format {name ?? "?"} is not supported");
	}
	public static Exception NoLayers() {
		return new ArgumentOutOfRangeException($"No Layers are present");
	}
}