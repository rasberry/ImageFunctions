using ImageFunctions.Core;
using System.Diagnostics;

namespace ImageFunctions.Gui.Models;

public sealed class GuiLogger : ICoreLog
{
	public delegate void LogEventHandler(object sender, GuiLoggerEventArgs args);
	public event LogEventHandler OnLogEvent;

	public LogCategory Category { get; set; }

	public void Debug(string m)
	{
		Trace.WriteLine($"D: {m}");
		if(Category > LogCategory.Debug) { return; }
		OnLogEvent?.Invoke(this, new GuiLoggerEventArgs(m, LogCategory.Debug));
	}

	public void Error(string m, Exception e = null)
	{
		if(Category > LogCategory.Error) { return; }
		OnLogEvent?.Invoke(this, new GuiLoggerEventArgs(m, LogCategory.Error));
	}

	public void Info(string m)
	{
		if(Category > LogCategory.Info) { return; }
		OnLogEvent?.Invoke(this, new GuiLoggerEventArgs(m, LogCategory.Info));
	}

	public void Message(string m)
	{
		if(Category > LogCategory.Message) { return; }
		OnLogEvent?.Invoke(this, new GuiLoggerEventArgs(m, LogCategory.Message));
	}

	public void Warning(string m)
	{
		if(Category > LogCategory.Warning) { return; }
		OnLogEvent?.Invoke(this, new GuiLoggerEventArgs(m, LogCategory.Warning));
	}
}

public sealed class GuiLoggerEventArgs : EventArgs
{
	public GuiLoggerEventArgs(string message, LogCategory category) : base()
	{
		Category = category;
		Message = message;
	}

	public LogCategory Category { get; private set; }
	public string Message { get; private set; }
}
