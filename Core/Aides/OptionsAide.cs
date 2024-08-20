using Rasberry.Cli;

namespace ImageFunctions.Core.Aides;

public static class OptionsAide
{
	/// <summary>
	/// Shortcut for printing a message when a parameter can't be parsed
	/// </summary>
	/// <typeparam name="T">Argument Type</typeparam>
	/// <param name="result">The result of an argument parse function</param>
	/// <returns>The result</returns>
	public static ParseResult<T> WhenInvalidTellDefault<T>(this ParseResult<T> result)
	{
		if(result.IsInvalid()) {
			Log.Error(Note.CouldNotParse(result.Name, result.Value), result.Error);
		}
		return result;
	}

	//TODO docs
	public static void SetCustomHelpPrinter(this IRegister reg, string @namespace, Func<IRegister,INameSpaceName,string> printer)
	{
		if (reg == null) {
			throw Squeal.ArgumentNull(nameof(reg));
		}
		if (printer == null) {
			throw Squeal.ArgumentNull(nameof(printer));
		}

		if (PrinterMap.ContainsKey(@namespace)) {
			throw Squeal.AlreadyMapped($"Printer for {@namespace}");
		}

		PrinterMap.Add(@namespace,printer);
	}

	public static string GetNameSpaceItemHelp(this IRegister reg, INameSpaceName item)
	{
		if (reg == null) {
			throw Squeal.ArgumentNull(nameof(reg));
		}
		if (item == null) {
			return null;
		}
		if (!PrinterMap.TryGetValue(item.NameSpace, out var func)) {
			return null;
		}
		var desc = func(reg,item);
		return desc;
	}

	static readonly Dictionary<string, Func<IRegister,INameSpaceName,string>> PrinterMap = new();
}