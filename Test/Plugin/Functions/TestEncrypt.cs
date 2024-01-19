using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestEncrypt : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.Encrypt);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 0.001;
		RunFunctionAndCompare(info, EncryptImageLoader);
	}

	void EncryptImageLoader(TestFunctionInfo info, string name, string folder = null)
	{
		//for the second test were decrypting so swap which images are being used
		if (info.OutName.EndsWith("2")) {
			//loading phase - null or images folder used
			if (folder == null || folder == "images") {
				folder = "control";
				name = $"{MyName}-{name}-1";
			}
			//testing phase - control folder used
			else {
				folder = "images";
				int start = MyName.Length + 1;
				int len = name.Length - start - 2;
				name = name.Substring(start,len);
			}
		}

		GetOrLoadResourceImage(info,name,folder);
	}

	static IEnumerable<TestFunctionInfo> GetTestInfoInternal()
	{
		foreach(var imgName in GetImageNames()) {
			foreach(var info in GetFunctionInfo(imgName)) {
				yield return info;
			}
		}
	}

	public static IEnumerable<object[]> GetData()
	{
		foreach(var info in GetTestInfoInternal()) {
			yield return new object[] { info };
		}
	}

	internal override IEnumerable<TestFunctionInfo> GetTestInfo()
	{
		return GetTestInfoInternal();
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo(string startImg)
	{
		yield return CreateTestInfo(1, startImg, new string[] { "-p", "1234" });
		yield return CreateTestInfo(2, startImg, new string[] { "-d", "-p", "1234" });
	}

	static TestFunctionInfo CreateTestInfo(int index, string startImg, string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{startImg}-{index}",
			ImageNames = new[] { startImg }
		};
	}

	public static IEnumerable<string> GetImageNames()
	{
		var list = new string[] { "toes","zebra" };
		return list;
	}
}
