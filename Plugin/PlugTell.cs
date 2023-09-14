using ImageFunctions.Core;

namespace ImageFunctions.Plugin;

// Put user facing messages here
public static class PlugTell
{
	public static void LayerMustHaveOne()
	{
		Log.Error("input layers must contain at least one image");
	}
	public static void MustHaveOnePriority() {
		Log.Error("You must provide at least one priority");
	}
	public static void PriorityMustBeNumber() {
		Log.Error("Each priority must be a number");
	}
}