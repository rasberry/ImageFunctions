using System.Drawing;
using ImageFunctions.Core;
using static ImageFunctions.Plugin.ImageComparer;

namespace ImageFunctions.Test;

public abstract class AbstractFunctionTest
{
	/// <summary>
	/// Provides the name of the registered function
	/// </summary>
	public abstract string FunctionName { get; }

	/// <summary>
	/// Run a test with the provided TestFunctionInfo. Note info.Layers should be set before
	///  calling this method.
	///  <example><code>
	///  using var layers = new Layers();
	///  info.Layers = layers;
	///  RunFunction(info);
	///  </code></example>
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
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
			args.Add(info.Size.Value.Width.ToString());
			args.Add(info.Size.Value.Height.ToString());
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

	/// <summary>
	/// Wrapper for running the RunFunction method and comapring the output to a reference image
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <param name="maxDiff">The maximum difference allowed between test and control images</param>
	public void RunFunctionAndCompare(TestFunctionInfo info, double maxDiff, SaveImageMode saveImage = SaveImageMode.None)
	{
		try {
			RunFunctionAndCompareInternal(info, maxDiff, saveImage);
		}
		finally {
			if (saveImage == SaveImageMode.SubjectOnly && info.Layers.Count > 1) {
				info.Layers.DisposeAt(0);
			}
			if (saveImage != SaveImageMode.None && info.Layers.Count > 0) {
				var path = Path.Combine(Setup.ProjectRootPath,"..",info.OutName);
				info.Options.Engine.Item.Value.SaveImage(info.Layers, path);
			}
		}
	}

	public enum SaveImageMode
	{
		None = 0,
		SubjectOnly = 1,
		SubjectAndControl = 2
	}

	void RunFunctionAndCompareInternal(TestFunctionInfo info, double maxDiff, SaveImageMode saveImage)
	{
		if (info.Layers == null) {
			throw Squeal.ArgumentNull("info.Layers - Layers should be managed from the test method");
		}

		RunFunction(info);
		Assert.AreEqual(true, info.Success);
		Assert.AreEqual(0, info.ExitCode);

		GetOrLoadResourceImage(info, info.OutName, "control");
		var dist = CompareTopTwoLayers(info);
		Log.Debug($"{info.OutName} dist = [{dist.R},{dist.G},{dist.B},{dist.A}] total={dist.Total}");

		Assert.IsTrue(dist.Total < maxDiff, $"Name = {info.OutName} Distance = {dist}");
	}

	/// <summary>
	/// Compares the top two layers contained in info.Layers
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <returns>The computed distance</returns>
	public ComponentDistance CompareTopTwoLayers(TestFunctionInfo info)
	{
		var layers = info.Layers;
		if (layers.Count < 2) {
			throw Squeal.LayerMustHaveAtLeast(2);
		}

		var one = layers[0];
		var two = layers[1];

		return CanvasDistance(one,two);
	}

	/// <summary>
	/// Compares the top two layers contained in info.Layers for equality
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <returns>True if the layers are the same otherwise false</returns>
	public bool AreTopLayersEqual(TestFunctionInfo info)
	{
		var layers = info.Layers;
		if (layers.Count < 2) {
			throw Squeal.LayerMustHaveAtLeast(2);
		}

		var one = layers[0];
		var two = layers[1];
		return AreCanvasEqual(one,two);
	}

	/// <summary>
	/// Load an image from the Resources folder within the project folder
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <param name="name">the file name of the image</param>
	/// <param name="folder">the subfolder within Resources folder that contains the image</param>
	/// <returns></returns>
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
	/// <summary>
	/// These are the test command line arguments
	/// </summary>
	public string[] Args { get; set; }

	/// <summary>
	/// Name of the control image used to compare the output
	/// </summary>
	public string OutName { get; set; }

	/// <summary>
	/// One or more names of source images to load into the info.Layers
	/// collection for use by the function
	/// </summary>
	public IEnumerable<string> ImageNames { get; set; }

	/// <summary>
	/// Exit code produced by the called function
	/// </summary>
	public int ExitCode;

	/// <summary>
	/// The boolean result of the called function
	/// </summary>
	public bool Success;

	/// <summary>
	/// Reference to a ILayers object for use with this test.
	/// </summary>
	public ILayers Layers { get; set; }

	/// <summary>
	/// Reference to the Core options object
	/// </summary>
	public ICoreOptions Options { get; set; }

	/// <summary>
	/// An optional size that will set the -# option before running the test
	///  This should match the control image size
	/// </summary>
	public Size? Size { get; set; }
}