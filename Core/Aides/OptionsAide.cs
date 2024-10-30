using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Core.Aides;

#pragma warning disable CA1715 // Prefix generic type parameter name with 'T'

public static class OptionsAide
{
	/// <summary>
	/// Shortcut for printing a message when a parameter can't be parsed
	/// </summary>
	/// <typeparam name="T">Argument Type</typeparam>
	/// <param name="result">The result of an argument parse function</param>
	/// <returns>The result</returns>
	public static ParseResult<T> WhenInvalidTellDefault<T>(this ParseResult<T> result, ICoreLog log)
	{
		if(log == null) { throw Squeal.ArgumentNull(nameof(log)); }
		if(result.IsInvalid()) {
			log.Error(Note.CouldNotParse(result.Name, result.Value), result.Error);
		}
		return result;
	}

	/// <summary>
	/// Assign a function to print a custom help message for items in a namespace
	/// </summary>
	/// <param name="reg">Register instance</param>
	/// <param name="namespace">namespace of item</param>
	/// <param name="printer">function which returns a custom help message for a given item</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException">if the namespace is already mapped</exception>
	public static void SetCustomHelpPrinter(this IRegister reg, string @namespace, Func<IRegister, INameSpaceName, string> printer)
	{
		if(reg == null) {
			throw Squeal.ArgumentNull(nameof(reg));
		}
		if(printer == null) {
			throw Squeal.ArgumentNull(nameof(printer));
		}

		if(PrinterMap.ContainsKey(@namespace)) {
			throw Squeal.AlreadyMapped($"Printer for {@namespace}");
		}

		PrinterMap.Add(@namespace, printer);
	}

	public static string GetNameSpaceItemHelp(this IRegister reg, INameSpaceName item)
	{
		if(reg == null) {
			throw Squeal.ArgumentNull(nameof(reg));
		}
		if(item == null) {
			return null;
		}
		if(!PrinterMap.TryGetValue(item.NameSpace, out var func)) {
			return null;
		}
		var desc = func(reg, item);
		return desc;
	}

	static readonly Dictionary<string, Func<IRegister, INameSpaceName, string>> PrinterMap = new();
	static readonly char[] RectPointDelims = new char[] { ' ', ',', 'x' };

	/// <summary>
	/// Parse a sequence of numbers into a point object
	/// Sequence may be seperated by space, comma or 'x'
	/// </summary>
	/// <typeparam name="P">Point, PointF, or PointD</typeparam>
	/// <param name="arg">argument value</param>
	/// <returns>A Point</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="OverflowException"></exception>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="FormatException"></exception>
	/// <exception cref="NotSupportedException"></exception>
	public static P ParsePoint<P>(string arg) where P : struct
	{
		var pt = typeof(P);

		if(pt == typeof(Point)) {
			return (P)(object)ParsePointInternal<Point, int>(arg, (a, b) => { return new Point(a, b); });
		}
		else if(pt == typeof(PointF)) {
			return (P)(object)ParsePointInternal<PointF, float>(arg, (a, b) => { return new PointF(a, b); });
		}
		else if(pt == typeof(PointD)) {
			return (P)(object)ParsePointInternal<PointD, double>(arg, (a, b) => { return new PointD(a, b); });
		}
		else {
			throw Squeal.NotSupported($"Type {pt.FullName}");
		}
	}

	static P ParsePointInternal<P, T>(string arg, Func<T, T, P> allocator) where T : IParsable<T>
	{
		//TODO maybe change ParseParams.Parser definition to match IParsable so we don't need a lambda
		var parser = new ParseParams.Parser<T>((string s) => T.Parse(s, null));
		var list = ExtraParsers.ParseSequence(arg, RectPointDelims, parser);
		if(list.Count != 2) { //must be two elements x,y
			throw Squeal.SequenceMustContain(2);
		}
		return allocator(list[0], list[1]);
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

	/// <summary>
	/// Helper to choose the engine from a name
	/// </summary>
	/// <param name="register">IRegister instance</param>
	/// <param name="engineName">name of the engine to select</param>
	/// <param name="engine">registered engine entry - will be null if none is found</param>
	/// <param name="errHandler">optional error handler Action</param>
	/// <returns>true if the engine was found otherwise false</returns>
	public static bool TrySelectEngine(
		this IRegister register, string engineName, [NotNull] ICoreLog log, out IRegisteredItem<Lazy<IImageEngine>> engine)
	{
		engine = null;
		var er = new EngineRegister(register);
		if(!String.IsNullOrWhiteSpace(engineName)) {
			if(!er.Try(engineName, out var engineEntry)) {
				log.Error(Note.NotRegistered(engineEntry.NameSpace, engineEntry.Name), null);
				return false;
			}
			engine = engineEntry;
		}
		else {
			engineName = EngineRegister.SixLaborsString;
			engine = er.Get(engineName);
		}

		return true;
	}


	/// <summary>
	/// Try to find a image format from it's name e.g. 'png', 'jpg'
	/// Note: formats are engine specific, but are typically the file extension (minus the dot)
	/// </summary>
	/// <param name="engine">Instance of IImageEngine</param>
	/// <param name="imageFormat">the name of the format</param>
	/// <param name="format">ImageFormat instance if found</param>
	/// <param name="errHandler">optional error handler Action</param>
	/// <returns>true if the format was found</returns>
	public static bool TryDetermineImageFormat(this IImageEngine engine, string imageFormat, ICoreLog log, out ImageFormat? format)
	{
		if(engine == null) { throw Squeal.ArgumentNull(nameof(engine)); }
		if(log == null) { throw Squeal.ArgumentNull(nameof(log)); }

		format = null;
		bool formatGiven = !String.IsNullOrWhiteSpace(imageFormat);
		ImageFormat? found = null;
		foreach(var f in engine.Formats()) {
			if(formatGiven && f.Name.EqualsIC(imageFormat)) {
				found = f;
			}
			else if(f.Name.EqualsIC("png")) { //default to png
				found = f;
			}
		}

		if(found == null) {
			log.Error(Note.NoImageFormatFound(imageFormat), null);
			return false;
		}

		return true;
	}
}
