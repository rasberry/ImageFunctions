using ImageFunctions.Core;

namespace ImageFunctions.Test;

public abstract class AbstractFunctionTest
{
	public abstract string FunctionName { get; }

	public void RunFunction(TestFunctionInfo info)
	{
		if (info.Layers == null) {
			throw Squeal.ArgumentNull("info.Layers - Layers should be managed from the test method");
		}

		//concat global arguments with test arguments
		var args = new List<string>();
		args.Add(FunctionName);

		//setup size if provided
		if (info.Size.HasValue) {
			args.Add("-#");
			args.Add(info.Size.Value.Item1.ToString());
			args.Add(info.Size.Value.Item2.ToString());
		}

		//args.Add("-e");
		//args.Add("imagemagick");

		//end of global args
		args.Add("--");
		args.AddRange(info.Args);

		//reset the global options and parse test options
		var options = new Options(Setup.Register);
		info.Options = options;
		var inst = new Program(Setup.Register, options, info.Layers);

		Assert.IsTrue(options.ParseArgs(args.ToArray(), null));
		Assert.IsTrue(options.ProcessOptions());

		//Load any specified images
		if (info.ImageNames?.Any() == true) {
			foreach(var name in info.ImageNames) {
				GetOrLoadResourceImage(info, name);
			}
		}

		//run the function and record the output
		bool worked = inst.TryRunFunction(out int code);
		info.ExitCode = code;
		info.Success = worked;
	}

	public double CompareTopTwoLayers(TestFunctionInfo info)
	{
		var layers = info.Layers;
		if (layers.Count < 2) {
			throw Squeal.LayerMustHaveAtLeast(2);
		}

		var one = layers[0];
		var two = layers[1];
		return Plugin.ImageComparer.CanvasDistance(one,two);
	}

	public bool AreTopLayersEqual(TestFunctionInfo info)
	{
		var layers = info.Layers;
		if (layers.Count < 2) {
			throw Squeal.LayerMustHaveAtLeast(2);
		}

		var one = layers[0];
		var two = layers[1];
		return Plugin.ImageComparer.AreCanvasEqual(one,two);
	}

	public ICanvas GetOrLoadResourceImage(TestFunctionInfo info, string name, string folder = "images")
	{
		string nameWithExt = Path.ChangeExtension(name,".png");
		string path = Path.Combine(Setup.ProjectRootPath,"../","Resources/",folder,nameWithExt);
		path = Path.GetFullPath(path); //normalize path so it doesn't look janky
		var layers = info.Layers;

		int index = layers.IndexOf(nameWithExt);
		if (index >= 0) {
			return layers[index];
		}

		if (!File.Exists(path)) {
			throw TestSqueal.FileNotFound(path);
		}

		info.Options.Engine.Item.Value.LoadImage(layers,path,nameWithExt);
		return layers.First();
	}
}

/// <summary>
/// Information needed for a single test run
/// </summary>
public class TestFunctionInfo
{
	public string[] Args { get; set; }
	public string OutName { get; set; }
	public IEnumerable<string> ImageNames { get; set; }
	public int ExitCode;
	public bool Success;
	public ILayers Layers { get; set; }
	public ICoreOptions Options { get; set; }
	public (int,int)? Size { get; set; }
}