using ImageFunctions.Core;

namespace ImageFunctions.ComfiUINodes;

public class LoggerForJob : ICoreLog
{
	public bool BeVerbose { get; set; }
	public List<string> LogMessages { get; private set; } = new();

	public void Debug(string m)
	{
		#if DEBUG
		LogMessages.Add($"D: {m}");
		#endif
	}

	public void Error(string m, Exception e = null)
	{
		var err = e == null ? "" : " " + e.Message;
		LogMessages.Add($"E: {m}{e}");
	}

	public void Info(string m)
	{
		if (BeVerbose) {
			LogMessages.Add($"I: {m}");
		}
	}

	public void Message(string m)
	{
		LogMessages.Add(m);
	}

	public void Warning(string m)
	{
		LogMessages.Add($"W: {m}");
	}
}