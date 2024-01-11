using System.Drawing;
using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestMaze : AbstractFunctionTest
{
	const int TestSizePixels = 256;
	const string MyName = nameof(Plugin.Functions.Maze);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 0.002;
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
		foreach(var info in GetFunctionInfo()) {
			yield return info;
		}
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo()
	{
		yield return CreateTestInfo( 1,new string[] { "-m", "1","-rs","5003" });
		yield return CreateTestInfo( 2,new string[] { "-m", "2","-rs","5009","-cc","tan","-wc","red" });
		yield return CreateTestInfo( 3,new string[] { "-m", "3","-rs","5011" });
		yield return CreateTestInfo( 4,new string[] { "-m", "4","-rs","5021" });
		yield return CreateTestInfo( 5,new string[] { "-m", "5","-rs","5023" });
		yield return CreateTestInfo( 6,new string[] { "-m", "6","-rs","5039" });
		yield return CreateTestInfo( 7,new string[] { "-m", "7","-rs","5051" });
		yield return CreateTestInfo( 8,new string[] { "-m", "8","-rs","5059" });
		yield return CreateTestInfo( 9,new string[] { "-m", "9","-rs","5077" });
		yield return CreateTestInfo(10,new string[] { "-m","10","-rs","5081" });
	}

	static TestFunctionInfo CreateTestInfo(int num,string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{num}",
			Size = new Size(TestSizePixels, TestSizePixels)
		};
	}
}
