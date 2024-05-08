using ImageFunctions.Core;

namespace ImageFunctions.Plugin;

// Put user facing messages here
public static class PlugNote
{
	public static string MustHaveOnePriority() {
		return "You must provide at least one priority";
	}
	public static string PriorityMustBeNumber() {
		return "Each priority must be a number";
	}
	public static string NotImplementedSpace(Functions.AllColors.Space space) {
		return $"Space {space} is not implemented";
	}
	public static string NotSupportedTypeByFunc(Type t, string funcName) {
		throw new NotSupportedException($"Type {t?.Name} is not supported by {funcName}");
	}
	public static string CannotParsePatterNumber(string snum) {
		return $"Unable to parse pattern number '{snum}'";
	}
	public static string PatternNumberGtrZero() {
		return "Pattern number must be greater than zero";
	}
	public static string SequenceMustContain(int num = 1) {
		var snum = Core.Tools.NumberToWord(num);
		return $"Sequence must contain {snum} element{(num == 1 ? "" : "s")}";
	}
	public static string SequenceMustContainOr(int num1, int num2) {
		var snum1 = Core.Tools.NumberToWord(num1);
		var snum2 = Core.Tools.NumberToWord(num2);
		return $"Sequence must contain {snum1} or {num2} elements";
	}
	public static string MustProvideAtLeast(string item, int num) {
		var snum = Core.Tools.NumberToWord(num);
		return $"Must provide at least {snum} {item}{(num == 1 ? "" : "s")}";
	}
	public static string MapSeconLayerNeedsTwoLayers() {
		return $"To map another image, {Note.LayerMustHaveAtLeast(2)}";
	}
}
