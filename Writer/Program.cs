using System.Text;
using ImageFunctions.Core;
using RazorEngineCore;

namespace ImageFunctions.Writer;

class Program
{
	static void Main(string[] args)
	{
		Setup();
		EnsureOutputFolderExists();
		//DrawRazor();
		DrawFunctions();
	}

	static void Setup()
	{
		var cp = System.Diagnostics.Process.GetCurrentProcess();
		cp.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

		Register = new Register();
		PluginLoader.RegisterPlugin(typeof(Core.Adapter).Assembly, Register);
		PluginLoader.RegisterPlugin(typeof(Plugin.Adapter).Assembly, Register);
	}

	static void Cleanup()
	{
		if (Register != null) {
			Register.Dispose();
			Register = null;
		}
	}
	internal static Register Register;

	static void DrawRazor()
	{
		RazorEngine engine = new();
		var path = Path.Combine(ProjectRoot,ProjectFolder,ViewFolder);
		var fileList = Directory.GetFiles(path,"*.razor");
		foreach(var f in fileList) {
			var text = File.ReadAllText(f);
			var template = engine.Compile(text);
			string txt = template.Run(GetModel(f));
			var outPath = GetOutputPath(f);
			Console.WriteLine(outPath);
			File.WriteAllText(outPath,txt);
		}
	}

	static void DrawFunctions()
	{
		var funReg = new FunctionRegister(Register);
		StringBuilder sb = new();

		foreach(string name in funReg.All()) {
			var reg = funReg.Get(name);
			var fun = reg.Item.Invoke(Register, null, null);

			fun.Usage(sb);
			Console.WriteLine(sb.ToString());
			sb.Clear();
		}
	}

	static void EnsureOutputFolderExists()
	{
		var outFolder = Path.Combine(ProjectFolder,OutFolder);
		if (!Directory.Exists(outFolder)) {
			Directory.CreateDirectory(outFolder);
		}
	}

	static object GetModel(string path)
	{
		return new WikiModel();
	}

	static string GetOutputPath(string path)
	{
		var name = Path.GetFileNameWithoutExtension(path);
		return Path.Combine(ProjectRoot,ProjectFolder,OutFolder,name);
	}

	static string ProjectRoot {
		get {
			if (RootFolder == null) {
				string root = AppContext.BaseDirectory;
				int i = 40; //max depth to check
				while(--i > 0) {
					string f = new DirectoryInfo(root).Name;
					if (string.Equals(f,nameof(ImageFunctions),StringComparison.CurrentCultureIgnoreCase)) {
						break;
					} else {
						root = new Uri(Path.Combine(root,"..")).LocalPath;
					}
				}
				RootFolder = root;
			}
			return RootFolder;
		}
	}
	static string RootFolder;

	const string ProjectFolder = "Writer";
	const string ViewFolder = "Views";
	const string OutFolder = "wiki";

}