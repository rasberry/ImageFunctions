namespace ImageFunctions.Core.Logging;

// Return exceptions to be thrown
public static class Squeal
{
	public static Exception AlreadyMapped(string @namespace)
	{
		return new ArgumentException(Note.AlreadyMapped(@namespace));
	}
	public static Exception AlreadyRegistered(string @namespace, string name)
	{
		return new ArgumentException(Note.ItemAlreadyRegistered(@namespace, name));
	}
	public static Exception ArgumentsMustBeEqual<T>(string name, T? v1, T? v2) where T : struct
	{
		return new ArgumentException(Note.MustBeEqual(name, v1, v2));
	}
	public static Exception ArgumentNull(string argName)
	{
		return new ArgumentNullException(argName);
	}
	public static Exception ArgumentNullOrEmpty(string argName)
	{
		return new ArgumentException(Note.MustNotBeNullOrEmpty(), argName);
	}
	public static Exception ArgumentOutOfRange(string argName)
	{
		return new ArgumentOutOfRangeException(argName);
	}
	public static Exception CouldNotLoadFile(string file, string extra)
	{
		return new FileLoadException(Note.CouldNotLoadFile(extra), file);
	}
	public static Exception EngineCannotDrawLines(string name)
	{
		return new NotSupportedException(Note.EngineCannotDrawLines(name));
	}
	public static Exception FormatIsNotSupported(string name)
	{
		return new NotSupportedException(Note.FormatIsNotSupported(name));
	}
	public static Exception IndexOutOfRange(string argName)
	{
		return new ArgumentOutOfRangeException(argName);
	}
	public static Exception InvalidArgument(string argName)
	{
		return new ArgumentException(Note.InvalidArgument(), argName);
	}
	public static Exception LayerMustHaveAtLeast(int count = 1)
	{
		return new ArgumentOutOfRangeException(Note.LayerMustHaveAtLeast(count));
	}
	public static Exception NoLayers()
	{
		return new ArgumentOutOfRangeException(Note.NoLayersPresent());
	}
	public static Exception NotMapped(string name)
	{
		return new KeyNotFoundException(Note.NotMapped(name));
	}
	public static Exception NotSupported(string prefix)
	{
		return new NotSupportedException(Note.NotSupported(prefix));
	}
	public static Exception NotSupportedChannelCount(int channelCount)
	{
		return new NotSupportedException(Note.ChannelCountNotSupported(channelCount));
	}
	public static Exception NotSupportedCMYK()
	{
		return new NotSupportedException(Note.NotSupported("CMYK"));
	}
	public static Exception SequenceMustContain(int num = 1)
	{
		return new ArgumentException(Note.SequenceMustContain(num));
	}
	public static Exception SequenceMustContainOr(int num1, int num2)
	{
		return new ArgumentException(Note.SequenceMustContainOr(num1, num2));
	}
	public static Exception LoggerAlreadyRegistered(int id)
	{
		return new ArgumentException(Note.LoggerAlreadyRegistered(id));
	}
}
