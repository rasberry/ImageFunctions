using ImageFunctions.Core;

namespace ImageFunctions.ComfiUINodes;

public class LoggerForJob : ICoreLog
{
	public List<string> LogMessages { get; private set; } = new();
	public LogCategory Category { get; set; }

	public void Debug(string m)
	{
		if(Category > LogCategory.Debug) { return; }
		LogMessages.Add($"D: {m}");
	}

	public void Error(string m, Exception e = null)
	{
		if(Category > LogCategory.Error) { return; }
		var err = e == null ? "" : " - " + e.Message;
		LogMessages.Add($"E: {m}{err}");
	}

	public void Info(string m)
	{
		if(Category > LogCategory.Info) { return; }
		LogMessages.Add($"I: {m}");
	}

	public void Message(string m)
	{
		if(Category > LogCategory.Message) { return; }
		LogMessages.Add(m);
	}

	public void Warning(string m)
	{
		if(Category > LogCategory.Warning) { return; }
		LogMessages.Add($"W: {m}");
	}
}
