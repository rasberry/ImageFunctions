namespace ImageFunctions.Plugin;

// Return exceptions to be thrown
public static class PlugSqueal
{
	public static Exception NotImplementedSpace(Functions.AllColors.Space space) {
		throw new NotImplementedException($"Space {space} is not implemented");
	}
	public static Exception NotSupportedTypeByFunc(Type t, string funcName) {
		throw new NotSupportedException($"Type {t?.Name} is not supported by {funcName}");
	}
	public static Exception OutOfRange(string name, string message = null) {
		return String.IsNullOrWhiteSpace(message)
			? new ArgumentOutOfRangeException(name)
			: new ArgumentOutOfRangeException(name, message);
	}
	public static Exception CannotParsePatterNumber(string snum) {
		return new ArgumentException($"Unable to parse pattern number '{snum}'");
	}
	public static Exception PatternNumberGtrZero() {
		return new ArgumentOutOfRangeException("Pattern number must be greater than zero");
	}
	public static Exception SequenceMustContain(int num = 1) {
		var snum = Core.Tools.NumberToWord(num);
		return new ArgumentException($"Sequence must contain {snum} element{(num == 1 ? "" : "s")}");
	}
	public static Exception SequenceMustContainOr(int num1, int num2) {
		var snum1 = Core.Tools.NumberToWord(num1);
		var snum2 = Core.Tools.NumberToWord(num2);
		return new ArgumentException($"Sequence must contain {snum1} or {num2} elements");
	}
}