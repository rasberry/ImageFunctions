namespace ImageFunctions.Core;

public static class Log
{
	public static void Message(string m)
	{
		Console.WriteLine(m);
	}

	public static void Info(string m)
	{
		if (BeVerbose) {
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine($"I: {m}");
			Console.ResetColor();
		}
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
		#if DEBUG
		Console.ForegroundColor = ConsoleColor.DarkGray;
		Console.WriteLine($"D: {m}");
		Console.ResetColor();
		#endif
	}

	public static bool BeVerbose { get; set; }
}
