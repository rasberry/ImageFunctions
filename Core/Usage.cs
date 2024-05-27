using Rasberry.Cli;

namespace ImageFunctions.Core;

public static class UsageRenderer
{
	public static StringBuilder RenderUsage(this StringBuilder sb, IUsageInfoProvider provider)
	{
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentNullException.ThrowIfNull(sb);

		var info = provider.GetUsageInfo();
		var desc = info.Description;
		if(desc.HasValue) {
			sb.ND(desc.Value.Indention, desc.Value.Description);
		}

		var pList = info.Parameters;
		if(pList != null) {
			foreach(var p in pList) {
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

		return sb;
	}
}

public interface IUsageInfoProvider
{
	UsageInfo GetUsageInfo();
}

public readonly record struct UsageParameter
{
	public UsageParameter(int indention, string name, string description, Type inputType)
	{
		this.Indention = indention;
		this.Name = name;
		this.Description = description;
		this.InputType = inputType;
	}

	public int Indention { get; init; }
	public string Name { get; init; }
	public string Description { get; init; }
	public Type InputType { get; init; }
	public IComparable Min { get; init; }
	public IComparable Max { get; init; }

}

public readonly record struct UsageEnumParameter
{
	public UsageEnumParameter(int indention, string title, Type enumType)
	{
		this.Indention = indention;
		this.Title = title;
		this.EnumType = enumType;
	}

	public int Indention { get; init; }
	public string Title { get; init; }
	public Func<object, string> DescriptionMap { get; init; } = null;
	public Func<object, string> NameMap { get; init; } = null;
	public bool ExcludeZero { get; init; } = false;
	public Type EnumType { get; init; }
}

public readonly record struct UsageDescription
{
	public UsageDescription(int indention, string description)
	{
		this.Indention = indention;
		this.Description = description;
	}

	public int Indention { get; init; }
	public string Description { get; init; }
}

public readonly record struct UsageInfo
{
	public UsageDescription? Description { get; init; }
	public IEnumerable<UsageParameter> Parameters { get; init; }
	public IEnumerable<UsageEnumParameter> EnumParameters { get; init; }
}
