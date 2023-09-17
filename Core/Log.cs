namespace ImageFunctions.Core;

public static class Log
{
	public static void Info(string m)
	{
		Console.WriteLine(m);
	}

	public static void Warning(string m)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"W: {m}");
		Console.ResetColor();
	}

	public static void Error(string m)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Error.WriteLine($"E: {m}");
		Console.ResetColor();
	}

	public static void Debug(string m)
	{
		//TODO check a setting here
		if (Options.BeVerbose) {
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine($"D: {m}");
			Console.ResetColor();
		}
	}
}
