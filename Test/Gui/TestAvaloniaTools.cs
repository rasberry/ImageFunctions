using ImageFunctions.Gui.Helpers;

namespace ImageFunctions.Test.Gui;

[TestClass]
public class TestAvaloniaTools
{
	[TestMethod]
	public void TestSplitCommandLine()
	{
		string cmd = "-a test1 -b test2 -c test3 test4";
		var args = AvaloniaTools.SplitCommandLine(cmd).ToArray();
		var expected = new string[] { "-a", "test1", "-b", "test2", "-c", "test3", "test4" };
		CollectionAssert.AreEqual(expected, args);
	}

	[TestMethod]
	public void TestSplitCommandLineQuotes()
	{
		string cmd = "-a \"test1\" -b \"test2\" -c \"test3\" \"test and 4\"";
		var args = AvaloniaTools.SplitCommandLine(cmd).ToArray();
		var expected = new string[] { "-a", "test1", "-b", "test2", "-c", "test3", "test and 4" };
		CollectionAssert.AreEqual(expected, args);
	}

	[TestMethod]
	public void TestSplitCommandLineNested()
	{
		string cmd = "-a \"test1\" -c \"test3\" \"test \\\"inner\\\" and 4\"";
		var args = AvaloniaTools.SplitCommandLine(cmd);
		var expected = new string[] { "-a", "test1", "-c", "test3", "test \"inner\" and 4" };
		CollectionAssert.AreEqual(expected, args.ToArray());
	}

	[TestMethod]
	public void TestSplitCommandLineNested2()
	{
		//dangling escape quote test
		string cmd = "-a \"test1\" \\\" -c \"test3\" \"test \\\"inner\\\" and 4\"";
		var args = AvaloniaTools.SplitCommandLine(cmd).ToArray();
		var expected = new string[] { "-a", "test1", "\"", "-c", "test3", "test \"inner\" and 4" };
		CollectionAssert.AreEqual(expected, args);
	}

	[TestMethod]
	public void TestSplitCommandLineNested3()
	{
		//slighly mangled quotes, but should glom the last tokens together
		string cmd = "-a \"test1\" \" test \\\"inner\\\" and 4\"";
		var args = AvaloniaTools.SplitCommandLine(cmd).ToArray();
		// System.Diagnostics.Trace.WriteLine($"len:{args.Length} j:{string.Join('|',args)}");
		var expected = new string[] { "-a", "test1", " test \"inner\" and 4" };
		CollectionAssert.AreEqual(expected, args);
	}
}
