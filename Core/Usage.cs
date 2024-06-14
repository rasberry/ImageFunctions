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
				if (p is UsageText ut && ut.AddNewLineBefore) { sb.WT(); }
				sb.ND(p.Indention, p.Name, p.Description);
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
				if (p is UsageText ut && ut.AddNewLineBefore) { sb.WT(); }
				sb.ND(p.Indention, p.Name, p.Description);
			}
		}

		return sb;
	}
}

//Note: the names need to match the namespace name exactly
[Flags]
public enum AuxiliaryKind
{
	None = 0,
	Sampler = 1,
	Metric = 2,
	Color3Space = 4,
	Color4Space = 8
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
	IComparable Min { get; }
	IComparable Max { get; }
	AuxiliaryKind Auxiliary { get; }
	object Default { get; }
}

public record UsageOne : UsageText, IUsageParameter
{
	public UsageOne(int indention, Type inputType, string name, string description)
		: base(indention, name, description)
	{
		this.InputType = inputType;
	}

	public Type InputType { get; init; }
	public IComparable Min { get; init; }
	public IComparable Max { get; init; }
	public AuxiliaryKind Auxiliary { get; init; }
	public object Default { get; init; }
}

public record UsageOne<T> : UsageOne
{
	public UsageOne(int indention, string name, string description)
		: base(indention, typeof(T), name, description)
	{
	}
}

public interface IUsageParameterTwo : IUsageParameter
{
	Type InputTypeTwo { get; }
	IComparable MinTwo { get; }
	IComparable MaxTwo { get; }
	object DefaultTwo { get; }
}

public record UsageTwo : UsageOne, IUsageParameterTwo
{
	public UsageTwo(int indention, Type inputTypeOne, Type inputTypeTwo, string name, string description)
		: base(indention, inputTypeOne, name, description)
	{
		this.InputTypeTwo = inputTypeTwo;
	}

	public Type InputTypeTwo { get; init; }
	public IComparable MinTwo { get; init; }
	public IComparable MaxTwo { get; init; }
	public object DefaultTwo { get; init; }
}

public record UsageTwo<T,U> : UsageTwo
{
	public UsageTwo(int indention, string name, string description)
		: base(indention, typeof(T), typeof(U), name, description)
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
