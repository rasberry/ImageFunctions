using ImageFunctions.Core;
using System.Drawing;

namespace ImageFunctions.Test.Core.Functions;

[TestClass]
public class TestGradient : AbstractFunctionTest
{
	const int TestSizePixels = 256;
	const string MyName = nameof(ImageFunctions.Core.Functions.Gradient);
	public override string FunctionName { get { return MyName; } }

	[TestMethod]
	[DynamicData(nameof(GetData))]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 100.000;
		//info.SaveImage = SaveImageMode.SubjectOnly;
		RunFunctionAndCompare(info);
	}

	public static IEnumerable<object[]> GetData()
	{
		foreach(var info in GetFunctionInfo()) {
			yield return new object[] { info };
		}
	}

	internal override IEnumerable<TestFunctionInfo> GetTestInfo()
	{
		return GetFunctionInfo();
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo()
	{
		yield return CreateTestInfo(0, new string[0]);
		yield return CreateTestInfo(1, new string[] { "--gradient", "Gimp Tube Red" });
		yield return CreateTestInfo(2, new string[] { "-ps", "40,20", "-pe", "250,120","-r" });
		yield return CreateTestInfo(3, new string[] { "-pps", "10%,10%", "-ppe", "90%,50%", "-s", "2.0"});
		yield return CreateTestInfo(4, new string[] { "-pps", "0.5,0.4", "-ppe", "0.9,0.5", "-g", "Conical" });
		yield return CreateTestInfo(5, new string[] { "--gradient", "Gimp Tube Red","-g","Radial","-d","ForBack","-o","0.5"});
	}

	static TestFunctionInfo CreateTestInfo(int num, string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{num}",
			Size = new Size(TestSizePixels, TestSizePixels)
		};
	}
}
