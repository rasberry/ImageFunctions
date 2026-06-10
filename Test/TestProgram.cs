using ImageFunctions.Test.Plugin.Functions;

namespace ImageFunctions.Test;

// TODO This is here to workaround VSCode and Zed test debugging inadequacies
//  VScode can't find a project name ImageFunctions_Test. don't know why it's putting the underscore in there
//  Zed doesn't have a mechanism to start a debug session directly from a test
internal sealed class TestProgram
{
	static void Main(string[] args)
	{
		var ctx = new CustomTestContext();
		Setup.Init(ctx);
		var test = new TestComplexPlot {
			TestContext = ctx
		};

		foreach(var info in TestComplexPlot.GetFunctionInfo()) {
			test.Test(info);
		}
	}

	class CustomTestContext : TestContext
	{
		public override IDictionary<string, object> Properties => new Dictionary<string, object>();

		public override void AddResultFile(string fileName)
		{
			Console.WriteLine($"T: AddResultFile {fileName}");
		}

		public override void DisplayMessage(MessageLevel messageLevel, string message)
		{
			Console.WriteLine($"T: DisplayMessage {messageLevel} {message}");
		}

		public override void Write(string message)
		{
			Console.Write(message);
		}

		public override void Write(string format, params object[] args)
		{
			var message = string.Format(format, args);
			Console.Write(message);
		}

		public override void WriteLine(string message)
		{
			Console.WriteLine($"W: {message}");
		}

		public override void WriteLine(string format, params object[] args)
		{
			var message = string.Format(format, args);
			Console.WriteLine(message);
		}
	}
}
