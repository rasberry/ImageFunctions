using RazorEngineCore;

namespace ImageFunctions.Writer;

class Program
{
	static void Main(string[] args)
	{
		//new RazorWriter().Test();
		DrawAll();
	}

	static void DrawAll()
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