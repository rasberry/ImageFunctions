namespace ImageFunctions.Plugin;

// Return exceptions to be thrown
public static class PlugSqueal
{
	public static Exception NotImplementedSpace(Functions.AllColors.Space space) {
		throw new NotImplementedException(PlugNote.NotImplementedSpace(space));
	}
	public static Exception NotSupportedTypeByFunc(Type t, string funcName) {
		throw new NotSupportedException(PlugNote.NotSupportedTypeByFunc(t,funcName));
	}
	public static Exception OutOfRange(string name, string message = null) {
		return String.IsNullOrWhiteSpace(message)
			? new ArgumentOutOfRangeException(name)
			: new ArgumentOutOfRangeException(name, message);
	}
	public static Exception CannotParsePatterNumber(string snum) {
		return new ArgumentException(PlugNote.CannotParsePatterNumber(snum));
	}
	public static Exception PatternNumberGtrZero() {
		return new ArgumentOutOfRangeException(PlugNote.PatternNumberGtrZero());
	}
	public static Exception SequenceMustContain(int num = 1) {
		return new ArgumentException(PlugNote.SequenceMustContain(num));
	}
	public static Exception SequenceMustContainOr(int num1, int num2) {
		return new ArgumentException(PlugNote.SequenceMustContainOr(num1,num2));
	}
	public static Exception MustProvideAtLeast(string item, int num) {
		return new ArgumentException(PlugNote.MustProvideAtLeast(item,num));
	}
}