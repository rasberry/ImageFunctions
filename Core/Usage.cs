#pragma warning disable CA1711 //Diabling warning about naming things with 'Enum' at the end
using ImageFunctions.Core.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Core;

/// <summary>
/// Produces a command-line style usage text describing input parameters
/// </summary>
public static class UsageRenderer
{
	/// <summary>
	/// Render method
	/// </summary>
	/// <param name="sb">A <c cref="StringBuilder"/> instance</param>
	/// <param name="provider">Instance of a <c cref="IUsageProvider"/></param>
	/// <returns>The given StringBuilder for chaining</returns>
	public static StringBuilder RenderUsage(this StringBuilder sb, IUsageProvider provider)
	{
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentNullException.ThrowIfNull(sb);

		var info = provider.GetUsageInfo();
		var altLookup = info.Alternates?.ToDictionary(i => i.Name) ?? null;

		var desc = info.Description;
		if(desc != null) {
			foreach(var d in desc.Descriptions) {
				sb.ND(desc.Indention, d);
			}
		}

		var pList = info.Parameters;
		if(pList != null) {
			foreach(var p in pList) {
				if(p.AddNewLineBefore) { sb.WT(); }
				var label = GetUsageLabel(p, altLookup);
				sb.ND(p.Indention, label, p.Description);
			}
		}

		var eList = info.EnumParameters;
		if(eList != null) {
			foreach(var e in eList) {
				sb.WT();
				sb.ND(e.Indention, e.Title);
				sb.PrintEnum(e.EnumType, e.Indention, e.DescriptionMap, e.NameMap, e.ExcludeZero);
			}
		}

		var sList = info.SuffixParameters;
		if(sList != null) {
			foreach(var p in sList) {
				if(p.AddNewLineBefore) { sb.WT(); }
				sb.ND(p.Indention, p.Name, p.Description);
			}
		}

		return sb;
	}

	static string GetUsageLabel(IUsageText p, Dictionary<string, IUsageAlt> altSet)
	{
		if(p is IUsageParameter iup) {
			string alt = null;
			if(altSet != null && altSet.TryGetValue(p.Name, out var altUsage)) {
				alt = altUsage.Alternate;
			}
			string name = String.IsNullOrWhiteSpace(alt) ? p.Name : $"{p.Name} / {alt}";
			var tt = iup.TypeText ?? MapTypeToText(iup.InputType, iup.IsNumberPct);
			var label = name + (String.IsNullOrEmpty(tt) ? "" : $" ({tt})");
			return label;
		}
		else {
			return p?.Name;
		}
	}

	static string MapTypeToText(Type t, bool isNumPct)
	{
		if(t == null) {
			throw Squeal.ArgumentNull(nameof(t));
		}

		t = t.UnWrapNullable();

		if(t.IsEnum) {
			return t.Name;
		}
		if(t.Is<bool>()) {
			return "";
		}
		else if(t.Is<ColorRGBA>() || t.Is<Color>()) {
			return "color";
		}
		else if(t.Is<Point>() || t.Is<PointF>()) {
			return "x,y";
		}
		else if(t.Is<Size>() || t.Is<SizeF>()) {
			return "w,h";
		}
		else if(t.Is<Rectangle>() || t.Is<RectangleF>()) {
			return "x,y,w,h";
		}
		if(t.IsNumeric()) {
				return isNumPct ? "number[%]" : "number";
			}
			else {
				throw Squeal.NotSupported($"Type {t.Name}");
			}
	}
}

/// <summary>
/// Provides a <c cref="Usage"/> object
/// </summary>
public interface IUsageProvider
{
	Usage GetUsageInfo();
}

/// <summary>
/// Describes a basic usage parameter
/// </summary>
public interface IUsageText
{
	/// <summary>Characters to indent</summary>
	int Indention { get; }
	/// <summary>Name of paramete. For command line this is they parameter key</summary>
	string Name { get; }
	/// <summary>Description of the parameter</summary>
	string Description { get; }
	/// <summary>Whether to include a new line before this option during rendering</summary>
	bool AddNewLineBefore { get; }
}

/// <inheritdoc />
public record UsageText : IUsageText
{
	public UsageText(int indention, string name, string description = null)
	{
		this.Indention = indention;
		this.Name = name;
		this.Description = description;
	}

	/// <inheritdoc />
	public int Indention { get; private set; }
	/// <inheritdoc />
	public string Name { get; private set; }
	/// <inheritdoc />
	public string Description { get; init; }
	/// <inheritdoc />
	public bool AddNewLineBefore { get; init; }
}

/// <summary>
/// A usage parameter with a default value and optionally min,max,numberPct
/// </summary>
public interface IUsageParameter : IUsageText
{
	/// <summary>The Type of the parameter value</summary>
	Type InputType { get; }
	/// <summary>Optional min for this value</summary>
	double? Min { get; }
	/// <summary>Optional max for this value</summary>
	double? Max { get; }
	/// <summary>The default value</summary>
	object Default { get; }
	/// <summary>Override the normal Type name</summary>
	string TypeText { get; }
	/// <summary>Specifies if this parameter represents a number%</summary>
	bool IsNumberPct { get; }
}

/// <inheritdoc />
public record UsageOne : UsageText, IUsageParameter
{
	public UsageOne(int indention, Type inputType, string name, string description = null)
		: base(indention, name, description)
	{
		this.InputType = inputType;
	}

	/// <inheritdoc />
	public Type InputType { get; private set; }
	/// <inheritdoc />
	public object Default { get; init; }
	/// <inheritdoc />
	public string TypeText { get; init; }
	/// <inheritdoc />
	public double? Min { get; init; }
	/// <inheritdoc />
	public double? Max { get; init; }
	/// <inheritdoc />
	public bool IsNumberPct { get; init; }
}
/// <inheritdoc />
public record UsageOne<T> : UsageOne
{
	public UsageOne(int indention, string name, string description = null)
		: base(indention, typeof(T), name, description)
	{
	}
}

/// <summary>
/// A usage parameter with a default value and optionally min,max,numberPct
/// </summary>
public interface IUsageMany : IUsageParameter
{
	/// <summary>How many of this parameter to allow</summary>
	int AllowCount { get; }
}

/// <inheritdoc />
public record UsageMany : UsageOne, IUsageMany
{
	public UsageMany(int indention, Type inputType, string name, string description = null, int count = 1)
		: base(indention, inputType, name, description)
	{
		AllowCount = count;
	}

	/// <inheritdoc />
	public int AllowCount { get; init; }
}

/// <inheritdoc />
public record UsageMany<T> : UsageMany
{
	public UsageMany(int indention, string name, string description = null, int count = 1)
		: base(indention, typeof(T), name, description, count)
	{
	}
}

/// <summary>
/// A usage parameter whos value corresponds to a registered namespace
/// </summary>
public record UsageRegistered : UsageOne
{
	public UsageRegistered(int indention, string name, string description)
		: base(indention, typeof(string), name, description)
	{
	}

	public string NameSpace { get; init; }
}

/// <summary>
/// A usage parameter whos value is an enum
/// </summary>
public interface IUsageEnum
{
	/// <summary>Characters to indent</summary>
	int Indention { get; }
	/// <summary>The title of the enum</summary>
	string Title { get; }
	/// <summary>A mapping from an enum value to a description</summary>
	Func<object, string> DescriptionMap { get; }
	/// <summary>A mapping from an enum value to a name</summary>
	Func<object, string> NameMap { get; }
	/// <summary>Skip the zero enum value when rendering</summary>
	bool ExcludeZero { get; }
	/// <summary>The Type of the enum</summary>
	Type EnumType { get; }
}

/// <inheritdoc />
public record UsageEnum : IUsageEnum
{
	public UsageEnum(int indention, Type enumType, string title)
	{
		this.Indention = indention;
		this.Title = title;
		this.EnumType = enumType;
	}

	/// <inheritdoc />
	public int Indention { get; private set; }
	/// <inheritdoc />
	public string Title { get; private set; }
	/// <inheritdoc />
	public Func<object, string> DescriptionMap { get; init; }
	/// <inheritdoc />
	public Func<object, string> NameMap { get; init; }
	/// <inheritdoc />
	public bool ExcludeZero { get; init; }
	/// <inheritdoc />
	public Type EnumType { get; private set; }
}

/// <inheritdoc />
public record UsageEnum<T> : UsageEnum
{
	public UsageEnum(int indention, string title)
		: base(indention, typeof(T), title)
	{
	}

	/// <inheritdoc />
	public new Func<T, string> DescriptionMap {
		get => arg => base.DescriptionMap(arg);
		init => base.DescriptionMap = arg => value((T)arg);
	}

	/// <inheritdoc />
	public new Func<T, string> NameMap {
		get => arg => base.DescriptionMap(arg);
		init => base.DescriptionMap = arg => value((T)arg);
	}
}

/// <summary>
/// Used to display text and not parameters
/// </summary>
public record UsageDescription
{
	public UsageDescription(int indention, string description = null)
	{
		this.Indention = indention;
		if(description != null) {
			this.Descriptions = new string[] { description };
		}
	}

	public UsageDescription(int indention, params string[] descriptions)
	{
		this.Indention = indention;
		this.Descriptions = descriptions;
	}

	/// <summary>Characters to indent</summary>
	public int Indention { get; private set; }
	/// <summary>A collection of description lines</summary>
	public IEnumerable<string> Descriptions { get; init; }
}

/// <summary>
/// An alternate name for a parameter
/// </summary>
public interface IUsageAlt
{
	/// <summary>The original name of the parameter</summary>
	string Name { get; }
	/// <summary>The alternate name of the parameter</summary>
	string Alternate { get; }
}

public record UsageAlt : IUsageAlt
{
	public UsageAlt(string name, string alt)
	{
		Name = name;
		Alternate = alt;
	}

	public string Name { get; init; }
	public string Alternate { get; init; }
}

/// <summary>
/// Represents a collection of usage items
/// </summary>
public record Usage
{
	/// <summary>The description for this collection</summary>
	public UsageDescription Description { get; init; }
	/// <summary>A collection of parameters</summary>
	public IEnumerable<IUsageText> Parameters { get; init; }
	/// <summary>The enum specific parameters</summary>
	public IEnumerable<IUsageEnum> EnumParameters { get; init; }
	/// <summary>Used for additional help text</summary>
	public IEnumerable<IUsageText> SuffixParameters { get; init; }
	/// <summary>A collection of alternate parameter names</summary>
	public IEnumerable<IUsageAlt> Alternates { get; init; }
}
