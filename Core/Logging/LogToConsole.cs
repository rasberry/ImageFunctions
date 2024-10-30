namespace ImageFunctions.Core.Logging;

public sealed class LogToConsole : ICoreLog
{
	public bool BeVerbose { get; set; }

	public void Message(string m)
	{
		Console.ResetColor();
		Console.WriteLine(m);
	}

	public void Info(string m)
	{
		if(BeVerbose) {
			WithColor($"I: {m}", ConsoleColor.Gray);
		}
	}

	public void Warning(string m)
	{
		WithColor($"W: {m}", ConsoleColor.Yellow, true);
	}

	public void Error(string m, Exception e = null)
	{
		string se = e == null ? "" : $" : {e.Message}";
		WithColor($"E: {m}{se}", ConsoleColor.Red, true);
	}

	public void Debug(string m)
	{
#if DEBUG
		WithColor($"D: {m}", ConsoleColor.DarkGray);
#endif
	}

	void WithColor(string m, ConsoleColor color, bool error = false)
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
