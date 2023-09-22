using System.Reflection.Metadata;

namespace ImageFunctions.Core;

// Put user facing messages here
public static class Tell
{
	public static void CannotFindFile(string path) {
		Log.Error($"cannot find input image '{path}'");
	}
	public static void CouldNotParse(string name, object val) {
		Log.Error($"invalid value '{val}' for '{name}'");
	}
	public static void InitingPlugin(Type t) {
		Log.Info($"Initializing plugin {t.FullName}");
	}
	public static void InvalidPassword() {
		Log.Error("password is missing or invalid");
	}
	public static void MissingArgument(string name) {
		Log.Error($"not enough arguments for '{name}'");
	}
	public static void MustBeBetween(string name, string low, string high) {
		Log.Error($"{name} must be between {low} and {high}");
	}
	public static void MustBeGreaterThanZero(string name, bool includeZero = false) {
		Log.Error($"{name} must be greater than {(includeZero?"or equal to ":"")}zero");
	}
	public static void MustBeSizeInBytes(string name, int sizeInBytes, bool isMin = false) {
		Log.Error($"{name} must be {(isMin?"at least ":"")}{sizeInBytes} bytes");
	}
	public static void MustHaveOnePriority() {
		Log.Error("You must provide at least one priority");
	}
	public static void MustProvideInput(string name) {
		Log.Error($"option '{name}' is required");
	}
	public static void NotRegistered(string @class, string name) {
		Log.Error($"{@class} '{name}' is not registered");
	}
	public static void NoImageFormatFound(string format) {
		var suffix = String.IsNullOrWhiteSpace(format)
			? ". Specify one using --format"
			: $" given '{format}'"
		;
		Log.Error($"Could not determine a usable format{suffix}");
	}
	public static void NoLayersToSave() {
		Log.Warning("There are no layers to save");
	}
	public static void PriorityMustBeNumber() {
		Log.Error("Each priority must be a number");
	}
	public static void PluginFileWarnLoading(string file, Exception e) {
		Log.Warning($"Problem loading library {file} {e.Message}");
	}
	public static void PluginTypeWarnLoading(Type t, Exception e) {
		Log.Warning($"Error instantiating plugin {t.FullName} {e.Message}");
	}
	public static void PluginInitFailed(Type t, Exception e) {
		Log.Warning($"Problem initializing plugin {t.FullName} {e.Message}");
	}
	public static void PluginFound(string file, string name) {
		Log.Info($"Plugin {name} Found {file}");
	}
	public static void Registering(string name) {
		Log.Info($"Registering {name}");
	}
}