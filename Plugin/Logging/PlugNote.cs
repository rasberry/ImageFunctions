using ImageFunctions.Core.Aides;

namespace ImageFunctions.Plugin;

// Put user facing messages here
public static class PlugNote
{
	public static string CannotParsePatterNumber(string snum)
	{
		return $"Unable to parse pattern number '{snum}'";
	}
	public static string MustHaveOnePriority()
	{
		return "You must provide at least one priority";
	}
	public static string MapSeconLayerNeedsTwoLayers()
	{
		return $"To map another image, {Note.LayerMustHaveAtLeast(2)}";
	}
	public static string MustBeInRange<T>(string name, T low, T high, bool lInc, bool hInc)
	{
		return $"{name} must be in the range {(lInc ? '[' : '(')}{low},{high}{(hInc ? ']' : ')')}";
	}
	public static string MustProvideAtLeast(string item, int num)
	{
		var snum = MathAide.NumberToWord(num);
		return $"Must provide at least {snum} {item}{(num == 1 ? "" : "s")}";
	}
	public static string NotImplementedSpace(Functions.AllColors.Space space)
	{
		return $"Space {space} is not implemented";
	}
	public static string NotSupportedTypeByFunc(Type t, string funcName)
	{
		throw new NotSupportedException($"Type {t?.Name} is not supported by {funcName}");
	}
	public static string PatternNumberGtrZero()
	{
		return "Pattern number must be greater than zero";
	}
	public static string PriorityMustBeNumber()
	{
		return "Each priority must be a number";
	}
}
