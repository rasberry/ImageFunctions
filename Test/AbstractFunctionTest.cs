using ImageFunctions.Cli;
using ImageFunctions.Core;
using ImageFunctions.Core.Logging;
using System.Drawing;
using static ImageFunctions.Plugin.ImageComparer;

namespace ImageFunctions.Test;

public abstract class AbstractFunctionTest
{
	/// <summary>
	/// Gets or sets the test context which provides
	/// information about and functionality for the current test run.
	/// </summary>
	public TestContext TestContext { get; set; }

	/// <summary>
	/// Provides the name of the registered function
	/// </summary>
	public abstract string FunctionName { get; }

	/// <summary>
	/// Option custom resource image loader to use when running tests
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <param name="name">name of the image to use for comparison</param>
	/// <param name="folder">the subfolder within Resources folder that contains the image</param>
	public delegate void CustomImageLoader(TestFunctionInfo info, string name, string folder = null);

	/// <summary>
	/// Used to specify tests that will be used as examples
	/// </summary>
	internal abstract IEnumerable<TestFunctionInfo> GetTestInfo();

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
	/// <param name="loader">Optional custom image loader</param>
	public void RunFunction(TestFunctionInfo info, CustomImageLoader loader = null)
	{
		var log = new TestLogger(TestContext);

		if(info.Layers == null) {
			throw Squeal.ArgumentNull("info.Layers - Layers should be managed from the test method");
		}

		//concat global arguments with test arguments
		var args = new List<string>();
		args.Add(FunctionName);

		//setup size if provided
		if(info.Size.HasValue) {
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
		var options = new Options(Setup.Register, log);
		if(System.Diagnostics.Debugger.IsAttached) {
			options.MaxDegreeOfParallelism = 1;
		}
		info.Options = options;

		if(info.Clerk == null) {
			info.Clerk = new FileClerk();
		}

		var inst = new Program(Setup.Register, options, info.Layers, info.Clerk, log);

		Assert.IsTrue(options.ParseArgs(args.ToArray(), null));
		Assert.IsTrue(options.ProcessOptions());

		//Load any specified images
		if(info.ImageNames?.Any() == true) {
			loader ??= GetOrLoadResourceImage;
			//reverse here since we're using a stack and we want the order
			// to
			foreach(var name in info.ImageNames.Reverse()) {
				loader(info, name);
			}
		}

		//run the function and record the output
		bool worked = inst.TryRunFunction(out int code);
		info.ExitCode = code;
		info.Success = worked;
	}

	/// <summary>
	/// Wrapper for running the RunFunction method and comapring the output to a reference image
	///  Writes images to disk based on saveImage input
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <param name="loader">Optional custom image loader</param>
	public void RunFunctionAndCompare(TestFunctionInfo info, CustomImageLoader loader = null)
	{
		var log = new TestLogger(TestContext);
		try {
			RunFunction(info, loader);
			Assert.AreEqual(true, info.Success);
			Assert.AreEqual(0, info.ExitCode);

			loader ??= GetOrLoadResourceImage;
			loader(info, info.OutName, "control");
			var dist = CompareTopTwoLayers(info);
			log.Info($"{info.OutName} dist = [{dist.R},{dist.G},{dist.B},{dist.A}] total={dist.Total}");

			Assert.IsTrue(dist.Total <= info.MaxDiff, $"Name = {info.OutName} Distance = {dist}");
		}
		finally {
			//remove the compare image
			if(info.SaveImage == SaveImageMode.SubjectOnly && info.Layers.Count > 1) {
				info.Layers.DisposeAt(0);
			}
			if(info.SaveImage != SaveImageMode.None && info.Layers.Count > 0) {
				var path = Path.Combine(Setup.ProjectRootPath, "..", info.OutName);
				info.Clerk.Location = path;
				info.Options.Engine.Item.Value.SaveImage(info.Layers, info.Clerk);
			}
		}
	}

	/// <summary>
	/// Compares the top two layers contained in info.Layers
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <returns>The computed distance</returns>
	public ComponentDistance CompareTopTwoLayers(TestFunctionInfo info)
	{
		var layers = info.Layers;
		if(layers.Count < 2) {
			throw Squeal.LayerMustHaveAtLeast(2);
		}

		var one = layers[0].Canvas;
		var two = layers[1].Canvas;

		return CanvasDistance(one, two);
	}

	/// <summary>
	/// Compares the top two layers contained in info.Layers for equality
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <returns>True if the layers are the same otherwise false</returns>
	public bool AreTopLayersEqual(TestFunctionInfo info)
	{
		var layers = info.Layers;
		if(layers.Count < 2) {
			throw Squeal.LayerMustHaveAtLeast(2);
		}

		var one = layers[0].Canvas;
		var two = layers[1].Canvas;
		return AreCanvasEqual(one, two);
	}

	/// <summary>
	/// Load an image from the Resources folder within the project folder
	/// </summary>
	/// <param name="info">An instance of the TestFunctionInfo object</param>
	/// <param name="name">the file name of the image</param>
	/// <param name="folder">the subfolder within Resources folder that contains the image</param>
	public void GetOrLoadResourceImage(TestFunctionInfo info, string name, string folder = null)
	{
		var path = GetResourceImagePath(name, folder);
		var layers = info.Layers;

		string nameWithExt = Path.ChangeExtension(name, ".png");
		int index = layers.IndexOf(nameWithExt);
		if(index >= 0) { return; }

		if(!File.Exists(path)) {
			throw TestSqueal.FileNotFound(path);
		}

		info.Clerk.Location = path;
		info.Options.Engine.Item.Value.LoadImage(layers, info.Clerk, nameWithExt);
	}

	protected string GetResourceImagePath(string name, string folder = null)
	{
		folder ??= "images";
		string nameWithExt = Path.ChangeExtension(name, ".png");
		string path = Path.Combine(Setup.ProjectRootPath, "../", "Resources/", folder, nameWithExt);
		path = Path.GetFullPath(path); //normalize path so it doesn't look janky
		return path;
	}
}

public enum SaveImageMode
{
	None = 0,
	SubjectOnly = 1,
	SubjectAndControl = 2
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

	/// <summary>
	/// The maximum difference allowed between test and control images
	/// </summary>
	public double MaxDiff { get; set; }

	/// <summary>
	/// Optional way to save images to disk. Usefull for debugging
	/// </summary>
	public SaveImageMode SaveImage { get; set; }

	/// <summary>
	/// Reference to the IFileClerk object
	/// </summary>
	public IFileClerk Clerk { get; set; }
}
