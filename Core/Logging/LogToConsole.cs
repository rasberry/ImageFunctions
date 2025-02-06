namespace ImageFunctions.Core.Logging;

public sealed class LogToConsole : ICoreLog
{
	public LogCategory Category { get; set; }

	public void Message(string m)
	{
		if(Category > LogCategory.Message) { return; }
		Console.ResetColor();
		Console.WriteLine(m);
	}

	public void Info(string m)
	{
		if(Category > LogCategory.Info) { return; }
		WithColor($"I: {m}", ConsoleColor.DarkCyan);
	}

	public void Warning(string m)
	{
		if(Category > LogCategory.Warning) { return; }
		WithColor($"W: {m}", ConsoleColor.Yellow, true);
	}

	public void Error(string m, Exception e = null)
	{
		if(Category > LogCategory.Error) { return; }
		string se = e == null ? "" : $" : {e.Message}";
		WithColor($"E: {m}{se}", ConsoleColor.Red, true);
	}

	public void Debug(string m)
	{
		if(Category > LogCategory.Debug) { return; }
		WithColor($"D: {m}", ConsoleColor.DarkGray);
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
