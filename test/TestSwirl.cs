using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestSwirl
	{
		const Activity Which = Activity.Swirl;
		const int num = (int)Which;
		static string name = Materials.GetTestImageNames(Which)[0];

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void Swirl(int index, string[] args)
		{
			using(var tempFile = Helpers.CreateTempPngFile())
			{
				string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
				string outFile = tempFile.TempFileName;
				string checkFile = Path.Combine(Helpers.ImgRoot,
					string.Format("img-{0}-{1}-{2}.png",num,name,index));

				var argsWithFiles = args.Append(inFile,outFile);
				Helpers.RunImageFunction(Which,argsWithFiles,outFile,checkFile);
			}
		}

		public static IEnumerable<object[]> GetData()
		{
			yield return new object[] {1, new string[0] };
			yield return new object[] {2, new string[] { "-rp","50%" } };
			yield return new object[] {3, new string[] { "-s","2" } };
			yield return new object[] {4, new string[] { "-ccw" } };
		}
	}
}
