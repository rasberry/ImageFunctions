using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestImgDiff : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.ImgDiff);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 61.0;
		//info.SaveImage = SaveImageMode.SubjectOnly;
		RunFunctionAndCompare(info);
	}

	public static IEnumerable<object[]> GetData()
	{
		foreach(var imgSet in GetImageNames()) {
			foreach(var info in GetFunctionInfo(imgSet)) {
				yield return new object[] { info };
			}
		}
	}

	internal override IEnumerable<TestFunctionInfo> GetTestInfo()
	{
		foreach(var imgName in GetImageNames()) {
			foreach(var info in GetFunctionInfo(imgName)) {
				yield return info;
			}
		}
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo((string,string) imgSet)
	{
		yield return CreateTestInfo(1, imgSet, new string[0]);
		yield return CreateTestInfo(2, imgSet, new string[] { "-i" });
		yield return CreateTestInfo(3, imgSet, new string[] { "-o", "0.9" });
		yield return CreateTestInfo(4, imgSet, new string[] { "-o","0.5","-c","red" });
	}

	static TestFunctionInfo CreateTestInfo(int index, (string,string) imgSet, string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{imgSet.Item1}-{imgSet.Item2}-{index}",
			ImageNames = new[] { imgSet.Item1, imgSet.Item2 }
		};
	}

	public static IEnumerable<(string,string)> GetImageNames()
	{
		yield return ("toes","toes-p");
		yield return ("scorpius","scorpius-p");
	}
}
