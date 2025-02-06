using ImageFunctions.Core;

namespace ImageFunctions.Test;

public sealed class TestLogger : ICoreLog
{
	public TestLogger(TestContext ctx)
	{
		Context = ctx;
	}

	readonly TestContext Context;
	public LogCategory Category { get; set; }

	public void Debug(string m)
	{
		if(Category > LogCategory.Debug) { return; }
		Context.WriteLine($"D: {m}");
	}

	public void Error(string m, Exception e = null)
	{
		if(Category > LogCategory.Error) { return; }
		string err = e == null ? " " : " " + e.Message;
		Context.WriteLine($"E: {m}{err}");
	}

	public void Info(string m)
	{
		if(Category > LogCategory.Info) { return; }
		Context.WriteLine($"I: {m}");
	}

	public void Message(string m)
	{
		if(Category > LogCategory.Message) { return; }
		Context.WriteLine($"M: {m}");
	}

	public void Warning(string m)
	{
		if(Category > LogCategory.Warning) { return; }
		Context.WriteLine($"W: {m}");
	}
}
