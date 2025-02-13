using ImageFunctions.Core;
using System.Runtime.CompilerServices;

namespace ImageFunctions.Test;

[TestClass]
public class Setup
{
	[AssemblyInitialize]
	public static void Init(TestContext ctx)
	{
		var log = new TestLogger(ctx);
		var cp = System.Diagnostics.Process.GetCurrentProcess();
		cp.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

		Register = new CoreRegister(log);
		PluginLoader.RegisterPlugin(log, typeof(Core.Adapter).Assembly, Register);
		PluginLoader.RegisterPlugin(log, typeof(Plugin.Adapter).Assembly, Register);
	}

	[AssemblyCleanup]
	public static void Cleanup()
	{
		if(Register != null) {
			Register.Dispose();
			Register = null;
		}
	}

	//internal static Program Instance;
	internal static CoreRegister Register;

	// https://stackoverflow.com/questions/816566/how-do-you-get-the-current-project-directory-from-c-sharp-code-when-creating-a-c
	internal static string ProjectRootPath { get { return runTimePath ??= CalculatePath(); } }
	const string FileRelativePath = nameof(Setup) + ".cs";
	static string runTimePath = null;

	static string CalculatePath()
	{
		string pathName = GetSourceFilePathName();
		return pathName.Substring(0, pathName.Length - FileRelativePath.Length);
	}

	static string GetSourceFilePathName([CallerFilePath] string callerFilePath = null)
	{
		return callerFilePath ?? "";
	}
}
