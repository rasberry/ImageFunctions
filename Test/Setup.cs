using System.Runtime.CompilerServices;
using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class Setup
{
	[AssemblyInitialize]
	public static void Init(TestContext ctx)
	{
		var cp = System.Diagnostics.Process.GetCurrentProcess();
		cp.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

		Register = new Register();
		//var layers = new Layers();
		//var options = new Options(register);
		//Log.BeVerbose = true;
		//var prog = new Program(register, options, layers);

		PluginLoader.RegisterPlugin(typeof(Core.Adapter).Assembly, Register);
		PluginLoader.RegisterPlugin(typeof(Plugin.Adapter).Assembly, Register);
	}

	[AssemblyCleanup]
	public static void Cleanup()
	{
		//if (Instance != null && Instance.Layers != null) {
		//	((Layers)Instance.Layers).Dispose();
		//	Instance.Layers = null;
		//}
	}

	//internal static Program Instance;
	internal static IRegister Register;

	// https://stackoverflow.com/questions/816566/how-do-you-get-the-current-project-directory-from-c-sharp-code-when-creating-a-c
	internal static string ProjectRootPath { get { return runTimePath ??= CalculatePath(); }}
	const string FileRelativePath = nameof(Setup) + ".cs";
	static string runTimePath = null;

	static string CalculatePath()
	{
		string pathName = GetSourceFilePathName();
		return pathName.Substring(0, pathName.Length - FileRelativePath.Length );
	}

	static string GetSourceFilePathName([CallerFilePath] string callerFilePath = null) {
		return callerFilePath ?? "";
	}
}