using ImageFunctions.Core;

namespace ImageFunctions.Test;

public sealed class TestLogger : ICoreLog
{
	public TestLogger(TestContext ctx)
	{
		Context = ctx;
	}

	readonly TestContext Context;
	public bool BeVerbose { get; set; }

	public void Debug(string m)
	{
#if DEBUG
		Context.WriteLine($"D: {m}");
#endif
	}

	public void Error(string m, Exception e = null)
	{
		string err = e == null ? " " : " " + e.Message;
		Context.WriteLine($"E: {m}{err}");
	}

	public void Info(string m)
	{
		if(BeVerbose) {
			Context.WriteLine($"I: {m}");
		}
	}

	public void Message(string m)
	{
		Context.WriteLine($"M: {m}");
	}

	public void Warning(string m)
	{
		Context.WriteLine($"W: {m}");
	}
}
