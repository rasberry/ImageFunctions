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
			WithColor($"I: {m}",ConsoleColor.Gray);
		}
	}

	public static void Warning(string m)
	{
		WithColor($"W: {m}",ConsoleColor.Yellow, true);
	}

	public static void Error(string m)
	{
		WithColor($"E: {m}",ConsoleColor.Red,true);
	}

	public static void Debug(string m)
	{
		#if DEBUG
		WithColor($"D: {m}",ConsoleColor.DarkGray);
		#endif
	}

	public static bool BeVerbose { get; set; }

	static void WithColor(string m, ConsoleColor color, bool error = false)
	{
		//using try-finally to make sure the console color gets reset
		// otherwise stopping the program (ctrl-c) can leave the console with a leftover color
		try {
			Console.ForegroundColor = color;
			var tw = error ? Console.Error : Console.Out;
			tw.WriteLine(m);
		}
		finally {
			Console.ResetColor();
		}
	}
}
