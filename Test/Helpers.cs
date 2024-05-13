using ImageFunctions.Core;

namespace ImageFunctions.Test;

public static class Helpers
{
	public static void Clear(this ILayers layers)
	{
		//pop the top until there's nothing left
		while(layers.Count > 0) {
			layers.DisposeAt(0);
		}
	}

	public static void AssertAreSimilar(ColorRGBA e, ColorRGBA a, double maxdiff = 0.0)
	{
		var dist = Plugin.ImageComparer.ColorDistance(e,a);
		Assert.IsTrue(dist.Total < maxdiff,
			$"Color Distance {dist} > {maxdiff}."
			+ $" Expected <{e.R},{e.G},{e.B},{e.A}>"
			+ $" Actual <{a.R},{a.G},{a.B},{a.A}>"
		);
	}

	public static int IdImage(ICanvas canvas)
	{
		int hash = 5381;
		for(int y=0; y < canvas.Height; y++) {
			for(int x = 0; x < canvas.Width; x++) {
				hash = (hash << 5) + hash + canvas[x,y].GetHashCode();
			}
		}
		return hash;
	}

	public static IEnumerable<TestFunctionInfo> GetTestsForFunction(string name)
	{
		if (TestInfo == null) {
			var testInfoType = typeof(TestFunctionInfo);
			var abstractTestType = typeof(AbstractFunctionTest);
			TestInfo = new Dictionary<string, List<TestFunctionInfo>>();

			var all = testInfoType.Assembly.GetTypes();
			foreach(var t in all) {
				if (!t.IsSubclassOf(abstractTestType)) { continue; }
				var inst = (AbstractFunctionTest)Activator.CreateInstance(t);
				var funName = inst.FunctionName;
				var infoList = inst.GetTestInfo().ToList();
				TestInfo.Add(funName, infoList);
			}
		}
		return TestInfo.TryGetValue(name, out var list) ? list : Enumerable.Empty<TestFunctionInfo>();
	}

	static Dictionary<string, List<TestFunctionInfo>> TestInfo;
}
