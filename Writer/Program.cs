using ImageFunctions.Core;
using RazorEngineCore;
using System.Text;

namespace ImageFunctions.Writer;

class Program
{
	static void Main(string[] args)
	{
		try {
			Setup();
			EnsureOutputFolderExists();
			DrawRazor();
			DrawFunctions();
		}
		finally {
			Cleanup();
		}
	}

	static void Setup()
	{
		var cp = System.Diagnostics.Process.GetCurrentProcess();
		cp.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

		Register = new Register();
		PluginLoader.RegisterPlugin(typeof(Core.Adapter).Assembly, Register);
		PluginLoader.RegisterPlugin(typeof(Plugin.Adapter).Assembly, Register);

		Engine = new();
	}

	static void Cleanup()
	{
		if(Register != null) {
			Register.Dispose();
			Register = null;
		}
	}
	static Register Register;
	static RazorEngine Engine;

	static void DrawRazor()
	{
		var fileList = Directory.GetFiles(ViewFolder, "*.razor");
		var funReg = new FunctionRegister(Register);
		var allFun = funReg.All().ToList();
		allFun.Sort();

		foreach(var path in fileList) {
			if(path.Contains("function.md.razor")) { continue; }
			var text = File.ReadAllText(path);
			var template = Engine.Compile(text);
			var model = new WikiModel { FunctionList = allFun };
			string renText = template.Run(model);
			var funName = Path.GetFileNameWithoutExtension(path);
			var outPath = Path.Combine(WikiFolder, funName);
			Log.Message($"Writing {Path.GetFullPath(outPath)}");
			File.WriteAllText(outPath, renText);
		}
	}

	static void DrawFunctions()
	{
		var funReg = new FunctionRegister(Register);
		var templateFile = Path.Combine(ViewFolder, "function.md.razor");
		var text = File.ReadAllText(templateFile);

		foreach(string name in funReg.All()) {
			var model = CreateSingleFunction(name, funReg);
			var template = Engine.Compile(text);
			string renText = template.Run(model);
			string outPath = Path.Combine(WikiFolder, $"f_{name}.md");
			Log.Message($"Writing {Path.GetFullPath(outPath)}");
			File.WriteAllText(outPath, renText);
		}
	}

	static WikiModel CreateSingleFunction(string name, FunctionRegister funReg)
	{
		var model = new WikiModel();
		StringBuilder sb = new();
		var reg = funReg.Get(name);
		var fun = reg.Item.Invoke(Register, null, null);

		fun.Options.Usage(sb, Register);
		model.Usage = sb.ToString();
		model.Table = new MarkdownTable();
		model.FunctionName = reg.Name;

		var testList = Test.Helpers.GetTestsForFunction(reg.Name);

		var colIndex = new Dictionary<string, int>();
		var rowIndex = new Dictionary<string, int>();

		List<string> headers = new() { "" };
		int col = 0, row = 0;
		foreach(var ti in testList) {
			var colLabel = MakeHeaderCell(ti.ImageNames);
			if(!colIndex.TryGetValue(colLabel, out var c)) {
				headers.Add(colLabel);
				c = ++col;
				colIndex.Add(colLabel, col);
			}

			string rowLabel = MakeRowCell(ti.Args);
			if(!rowIndex.TryGetValue(rowLabel, out var r)) {
				model.Table.SetCell(row, 0, rowLabel);
				rowIndex.Add(rowLabel, row);
				model.Table.SetCell(row, c, MakeThumb(ti.OutName));
				r = ++row;
			}
			else {
				model.Table.SetCell(r, c, MakeThumb(ti.OutName));
			}
		}
		model.Table.SetHeader(headers);
		return model;
	}

	static string MakeHeaderCell(IEnumerable<string> imageNames)
	{
		string label = imageNames == null ? "Images" : string.Join(", ", imageNames);
		if(String.IsNullOrWhiteSpace(label)) { return "Images"; }
		return label;
	}
	static string MakeRowCell(IEnumerable<string> args)
	{
		string label = args == null ? "Default" : string.Join(' ', args);
		if(String.IsNullOrWhiteSpace(label)) { return "Default"; }
		return label;
	}

	static string MakeThumb(string name)
	{
		string imgPath = Path.Combine(ImgFolder, $"{name}.png");
		string path = Path.GetRelativePath(WikiFolder, imgPath).Replace("\\", "/");

		if(!File.Exists(imgPath)) {
			string srcImage = Path.Combine(SrcImageFolder, $"{name}.png");

			//ugly hack for Encrypt function
			if(name == "Encrypt-toes-2" || name == "Encrypt-zebra-2") {
				int start = "Encrypt".Length + 1;
				int len = name.Length - start - 2;
				string newName = name.Substring(start, len);
				srcImage = Path.Combine(ProjectRoot, "Resources", "images", $"{newName}.png");
			}

			Log.Message($"Copying {Path.GetFullPath(srcImage)} -> {Path.GetFullPath(imgPath)}");
			File.Copy(srcImage, imgPath);
		}

		return $"![{name}]({path} \"{name}\")";
	}

	static void EnsureOutputFolderExists()
	{
		if(!Directory.Exists(WikiFolder)) {
			Log.Message($"Making folder {Path.GetFullPath(WikiFolder)}");
			Directory.CreateDirectory(WikiFolder);
		}

		if(!Directory.Exists(ImgFolder)) {
			Log.Message($"Making folder {Path.GetFullPath(ImgFolder)}");
			Directory.CreateDirectory(ImgFolder);
		}
	}

	//TODO change this to what the Test project does
	static string ProjectRoot {
		get {
			if(RootFolder == null) {
				string root = AppContext.BaseDirectory;
				int i = 40; //max depth to check
				while(--i > 0) {
					string f = new DirectoryInfo(root).Name;
					if(string.Equals(f, nameof(ImageFunctions), StringComparison.CurrentCultureIgnoreCase)) {
						break;
					}
					else {
						root = new Uri(Path.Combine(root, "..")).LocalPath;
					}
				}
				RootFolder = root;
			}
			return RootFolder;
		}
	}
	static string RootFolder;

	static string ProjectFolder { get { return Path.Combine(ProjectRoot, "Writer"); } }
	static string ViewFolder { get { return Path.Combine(ProjectRoot, "Writer", "Views"); } }
	static string WikiFolder { get { return Path.Combine(ProjectRoot, "wiki"); } }
	static string ImgFolder { get { return Path.Combine(ProjectRoot, "wiki", "img"); } }
	static string SrcImageFolder { get { return Path.Combine(ProjectRoot, "Resources", "Control"); } }
}
