namespace ImageFunctions.Core;

public static class Log
{
	public static void Info(string m)
	{
		if (!Options.SuppressInfo) {
			Console.WriteLine(m);
		}
	}

	public static void Warning(string m)
	{
		Console.WriteLine($"W: {m}");
	}

	public static void Error(string m)
	{
		Console.Error.WriteLine($"E: {m}");
	}

	public static void Debug(string m)
	{
		//TODO check a setting here
		Console.WriteLine($"D: {m}");
	}
}