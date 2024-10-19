using ImageFunctions.Core.Aides;

namespace ImageFunctions.Core.Logging;

public static class Note
{
	public static string AlreadyMapped(string @namespace)
	{
		return $"{@namespace} has already been mapped";
	}
	public static string CannotFindInputImage(string path)
	{
		return $"cannot find input image '{path}'";
	}
	public static string ChannelCountNotSupported(int channelCount)
	{
		return NotSupported($"Channel Count {channelCount}");
	}
	public static string ComponentNotFound(string name)
	{
		return $"Component '{name}' was not found";
	}
	public static string CouldNotLoadFile(string extra)
	{
		return $"Could not load file ({extra})";
	}
	public static string CouldNotParse(string name, object val = null)
	{
		string sval = val == null ? "" : $" '{val}'";
		return $"invalid value{sval} for '{name}'";
	}
	public static string EngineCannotDrawLines(string name)
	{
		return $"Engine {name} does not support drawing lines";
	}
	public static string FormatIsNotSupported(string name)
	{
		return NotSupported($"format {name ?? "?"}");
	}
	public static string InitializingPlugin(Type t)
	{
		return $"Initializing plugin {t?.FullName}";
	}
	public static string InvalidArgument()
	{
		return "Invalid argument";
	}
	public static string InvalidPassword()
	{
		return "password is missing or invalid";
	}
	public static string ItemAlreadyRegistered(string @namespace, string name)
	{
		return $"Item {@namespace}.{name} is already registered";
	}
	public static string ItemFoundWithWrongType(string @namespace, string name, string expectedType, string foundType)
	{
		return $"Item {@namespace}.{name} was found but has incorrect type. Expected: '{expectedType}' Found: '{foundType}'";
	}
	public static string LayerMustHaveAtLeast(int count = 1)
	{
		var word = MathAide.NumberToWord(count);
		return $"layers collection must contain at least {word} layer{(count > 1 ? "s" : "")}";
	}
	public static string MissingArgument(string name)
	{
		return $"not enough arguments for '{name}'";
	}
	public static string MustBeBetween(string name, string low, string high)
	{
		return $"{name} must be between {low} and {high}";
	}
	public static string MustBeEqual<T>(string name, T? v1, T? v2) where T : struct
	{
		string vals = (v1.HasValue && v2.HasValue) ? $" [{v1} : {v2}]" : "";
		return $"{name}(s) must be equal{vals}";
	}
	public static string MustBeGreaterThan<T>(string name, T number, bool inclusive = false)
	{
		if (number == null) {
			throw Squeal.ArgumentNull(nameof(number));
		}
		var c = System.Globalization.CultureInfo.InvariantCulture;
		int num = number is IConvertible n ? n.ToInt32(c) : -1;
		string w = num >= 0 && num <= 9
			? MathAide.NumberToWord(num)
			: number.ToString()
		;
		return $"{name} must be greater than {(inclusive ? "or equal to " : "")}{w}";
	}
	public static string MustNotBeNullOrEmpty()
	{
		return "must not be null or empty";
	}
	public static string MustBeSizeInBytes(string name, int sizeInBytes, bool isMin = false)
	{
		return $"{name} must be {(isMin ? "at least " : "")}{sizeInBytes} bytes";
	}
	public static string MustHaveOnePriority()
	{
		return "You must provide at least one priority";
	}
	public static string MustProvideInput(string name)
	{
		return $"option '{name}' is required (Note: function arguments must be included after '--')";
	}
	public static string NotMapped(string name)
	{
		return $"{name} was not found";
	}
	public static string NotRegistered(string @class, string name)
	{
		return $"{@class} '{name}' is not registered";
	}
	public static string NoImageFormatFound(string format)
	{
		var suffix = String.IsNullOrWhiteSpace(format)
			? ". Specify one using --format"
			: $" given '{format}'"
		;
		return $"Could not determine a usable format{suffix}";
	}
	public static string NoLayersPresent()
	{
		return "No Layers are present";
	}
	public static string NoLayersToSave()
	{
		return "No Layers to Save";
	}
	public static string NotSupported(string prefix)
	{
		return $"{prefix} is not supported";
	}
	public static string PriorityMustBeNumber()
	{
		return "Each priority must be a number";
	}
	public static string PluginFileWarnLoading(string file, Exception e)
	{
		return $"Problem loading library {file} {e?.Message}";
	}
	public static string PluginTypeWarnLoading(Type t, Exception e)
	{
		return $"Error instantiating plugin {t?.FullName} {e?.Message}";
	}
	public static string PluginInitFailed(Type t, Exception e)
	{
#if DEBUG
		string err = e?.ToString();
#else
		if (e is System.Reflection.TargetInvocationException) {
			e = e.InnerException;
		}
		string err = e?.Message;
#endif
		return $"Problem initializing plugin {t?.FullName} {err}";
	}
	public static string PluginFound(string file, string name)
	{
		return $"Plugin {name} Found {file}";
	}
	public static string PluginNotFound(string file, string name)
	{
		return $"Plugin {name} NOT Found {file}";
	}
	public static string Registering(string @namespace, string name)
	{
		return $"Registering {@namespace}.{name}";
	}
	public static string SequenceMustContain(int num = 1)
	{
		var snum = MathAide.NumberToWord(num);
		return $"Sequence must contain {snum} element{(num == 1 ? "" : "s")}";
	}
	public static string SequenceMustContainOr(int num1, int num2)
	{
		var snum1 = MathAide.NumberToWord(num1);
		var snum2 = MathAide.NumberToWord(num2);
		return $"Sequence must contain {snum1} or {num2} elements";
	}
}
