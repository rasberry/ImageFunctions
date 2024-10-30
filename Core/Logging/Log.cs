namespace ImageFunctions.Core.Logging;

// /// <summary>
// /// Log functionality. If a logger is not registered a default console logger is used
// /// = loggers are tied to the thread that registered them, so you may need to register a logger for
// /// each thread you spawn, or use DefaultLogger to use one logger for all threads.
// /// = loggers registered on a thread will run before DefaultLogger
// /// </summary>
// public static class Log
// {
// 	/// <summary>Print a normal message</summary>
// 	/// <param name="m">the message</param>
// 	public static void Message(string m)
// 	{
// 		GetLogger().Message(m);
// 	}
// 	/// <summary>Print an info message</summary>
// 	/// <param name="m">the message</param>
// 	public static void Info(string m)
// 	{
// 		GetLogger().Info(m);
// 	}
// 	/// <summary>Print an warning message</summary>
// 	/// <param name="m">the message</param>
// 	public static void Warning(string m)
// 	{
// 		GetLogger().Warning(m);
// 	}
// 	/// <summary>Print an debug message - these are only shown in DEBUG builds</summary>
// 	/// <param name="m">the message</param>
// 	public static void Debug(string m)
// 	{
// 		GetLogger().Debug(m);
// 	}
// 	/// <summary>Print an error message</summary>
// 	/// <param name="m">the message</param>
// 	/// <param name="e">optional exception to show more information</param>
// 	public static void Error(string m, Exception e = null)
// 	{
// 		GetLogger().Error(m,e);
// 	}

// 	/// <summary>Enables info messages to be shown</summary>
// 	public static bool BeVerbose {
// 		get {
// 			return GetLogger().BeVerbose;
// 		}
// 		set {
// 			GetLogger().BeVerbose = value;
// 		}
// 	}

// 	static ICoreLog GetLogger()
// 	{
// 		int id = Environment.CurrentManagedThreadId;
// 		if (!Store.TryGetValue(id, out var logger)) {
// 			return DefaultLogger ?? _defaultLogger;
// 		}
// 		return logger;
// 	}

// 	/// <summary>Register a logger for the current thread</summary>
// 	public static void RegisterLogger(ICoreLog logger)
// 	{
// 		int id = Environment.CurrentManagedThreadId;

// 		if (!Store.TryAdd(id, logger)) {
// 			throw Squeal.LoggerAlreadyRegistered(id);
// 		}
// 	}

// 	/// <summary>Assign a logger which is used if no other loggers are registered</summary>
// 	public static ICoreLog DefaultLogger { get; set; }


// 	static readonly ConcurrentDictionary<int, ICoreLog> Store = new();
// 	static readonly LogForConsole _defaultLogger = new();
// }

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
