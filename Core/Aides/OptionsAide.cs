using Rasberry.Cli;
using System.Drawing;

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

	static char[] RectPointDelims = new char[] { ' ', ',', 'x' };

	/// <summary>
	/// Parse a sequence of numbers into a point object
	/// Sequence may be seperated by space, comma or 'x'
	/// </summary>
	/// <param name="arg">argument value</param>
	/// <returns>A Point</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="OverflowException"></exception>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="FormatException"></exception>
	public static Point ParsePoint(string arg)
	{
		var parser = new ParseParams.Parser<int>(int.Parse);
		var list = ExtraParsers.ParseSequence(arg, RectPointDelims, parser);
		if(list.Count != 2) { //must be two elements x,y
			throw Squeal.SequenceMustContain(2);
		}
		return new Point(list[0], list[1]);
	}

	/// <summary>
	/// Parse a sequence of numbers into a rectangle object
	/// Sequence may be seperated by space, comma or 'x'
	/// </summary>
	/// <param name="arg">argument value</param>
	/// <returns>A Rectangle</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="OverflowException"></exception>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="FormatException"></exception>
	public static Rectangle ParseRectangle(string arg)
	{
		var parser = new ParseParams.Parser<int>(int.Parse);
		var list = ExtraParsers.ParseSequence(arg, RectPointDelims, parser);
		if(list.Count != 2 && list.Count != 4) { //must be two or four elements w,h / x,y,w,h
			throw Squeal.SequenceMustContainOr(2, 4);
		}
		if(list.Count == 2) {
			//assume width / height for 2 elements
			return new Rectangle(0, 0, list[0], list[1]);
		}
		else {
			//x, y, w, h
			return new Rectangle(list[0], list[1], list[2], list[3]);
		}
	}

	/// <summary>
	/// Attempts to parse a color from name for hex value. for example 'red' or '#FF0000'
	/// </summary>
	/// <param name="arg">input string</param>
	/// <returns>ColorRGBA object</returns>
	public static ColorRGBA ParseColor(string arg)
	{
		var sdc = ExtraParsers.ParseColor(arg);
		return ColorRGBA.FromRGBA255(sdc.R, sdc.G, sdc.B, sdc.A);
	}
}