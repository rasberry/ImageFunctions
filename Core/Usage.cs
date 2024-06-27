using Rasberry.Cli;

namespace ImageFunctions.Core;

public static class UsageRenderer
{
	public static StringBuilder RenderUsage(this StringBuilder sb, IUsageProvider provider)
	{
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentNullException.ThrowIfNull(sb);

		var info = provider.GetUsageInfo();
		var desc = info.Description;
		if(desc != null) {
			foreach(var d in desc.Descriptions) {
				sb.ND(desc.Indention, d);
			}
		}

		var pList = info.Parameters;
		if(pList != null) {
			foreach(var p in pList) {
				if (p.AddNewLineBefore) { sb.WT(); }
				var label = GetUsageLabel(p);
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
		if (sList != null) {
			foreach(var p in sList) {
				if (p.AddNewLineBefore) { sb.WT(); }
				sb.ND(p.Indention, p.Name, p.Description);
			}
		}

		return sb;
	}

	public static string GetUsageLabel(IUsageText p)
	{
		if (p is IUsageParameterTwo iup2) {
			var tt = iup2.TypeText ?? MapTypeToText(iup2.InputType, iup2.IsNumberPct);
			var suffix = String.IsNullOrEmpty(tt) ? "" : $" ({tt})";
			var label = p.Name + suffix + suffix;
			return label;
		}
		else if (p is IUsageParameter iup) {
			var tt = iup.TypeText ?? MapTypeToText(iup.InputType, iup.IsNumberPct);
			var label = p.Name + (String.IsNullOrEmpty(tt) ? "" : $" ({tt})");
			return label;
		}
		else {
			return p?.Name;
		}
	}

	public static string MapTypeToText(Type t, bool isNumPct)
	{
		if (t == null) {
			throw Squeal.ArgumentNull(nameof(t));
		}

		t = t.UnWrapNullable();

		if (t.IsEnum) {
			return t.Name;
		}
		if (t.IsBool()) {
			return "";
		}
		else if (t.IsColorRGBA() || t.IsColor()) {
			return "color";
		}
		else if (t.IsPoint()) {
			return "x,y";
		}
		if (t.IsNumeric()) {
			return isNumPct ? "number[%]" : "number";
		}
		else {
			throw Squeal.NotSupported($"Type {t.Name}");
		}
	}
}

public class GetSet<T>
{
	public T Get() => Value;
	public void Set(T val) => Value = val;
	T Value;
}

public interface IUsageProvider
{
	Usage GetUsageInfo();
}

public interface IUsageText
{
	int Indention { get; }
	string Name { get; }
	string Description { get; }
	bool AddNewLineBefore { get; }
}

public record UsageText : IUsageText
{
	public UsageText(int indention, string name, string description = null)
	{
		this.Indention = indention;
		this.Name = name;
		this.Description = description;
	}

	public int Indention { get; init; }
	public string Name { get; init; }
	public string Description { get; init; }
	public bool AddNewLineBefore { get; init; }
}

public interface IUsageParameter : IUsageText
{
	Type InputType { get; }
	double? Min { get; }
	double? Max { get; }
	object Default { get; }
	string TypeText { get; }
	bool IsNumberPct { get; }
}

public record UsageOne : UsageText, IUsageParameter
{
	public UsageOne(int indention, Type inputType, string name, string description)
		: base(indention, name, description)
	{
		this.InputType = inputType;
	}

	public Type InputType { get; init; }
	public object Default { get; init; }
	public string TypeText { get; init; }
	public double? Min { get; init; }
	public double? Max { get; init; }
	public bool IsNumberPct { get; init; }
}

public record UsageOne<T> : UsageOne
{
	public UsageOne(int indention, string name, string description)
		: base(indention, typeof(T), name, description)
	{
	}
}

public record UsageRegistered : UsageOne
{
	public UsageRegistered(int indention, string name, string description)
		: base(indention, typeof(string), name, description)
	{
	}

	public string NameSpace { get; init; }
}

public interface IUsageParameterTwo : IUsageParameter
{
	object DefaultTwo { get; }
}

public record UsageTwo : UsageOne, IUsageParameterTwo
{
	public UsageTwo(int indention, Type inputType, string name, string description)
		: base(indention, inputType, name, description)
	{
	}

	public object DefaultTwo { get; init; }
}

public record UsageTwo<T> : UsageTwo
{
	public UsageTwo(int indention, string name, string description)
		: base(indention, typeof(T), name, description)
	{
	}
}

public interface IUsageEnum
{
	int Indention { get; init; }
	string Title { get; init; }
	Func<object, string> DescriptionMap { get; init; }
	Func<object, string> NameMap { get; init; }
	bool ExcludeZero { get; init; }
	Type EnumType { get; init; }

}

public record UsageEnum : IUsageEnum
{
	public UsageEnum(int indention, Type enumType, string title)
	{
		this.Indention = indention;
		this.Title = title;
		this.EnumType = enumType;
	}

	public int Indention { get; init; }
	public string Title { get; init; }
	public Func<object, string> DescriptionMap { get; init; }
	public Func<object, string> NameMap { get; init; }
	public bool ExcludeZero { get; init; }
	public Type EnumType { get; init; }
}

public record UsageEnum<T> : UsageEnum
{
	public UsageEnum(int indention, string title)
		: base(indention, typeof(T), title)
	{
	}
}

public record UsageDescription
{
	public UsageDescription(int indention, string description)
	{
		this.Indention = indention;
		this.Descriptions = new string[] { description };
	}

	public UsageDescription(int indention, params string[] descriptions)
	{
		this.Indention = indention;
		this.Descriptions = descriptions;
	}

	public int Indention { get; init; }
	public IEnumerable<string> Descriptions { get; init; }
}

public record Usage
{
	public UsageDescription Description { get; init; }
	public IEnumerable<IUsageText> Parameters { get; init; }
	public IEnumerable<IUsageEnum> EnumParameters { get; init; }
	public IEnumerable<IUsageText> SuffixParameters { get; init; }
}
