using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace test
{
	[TestClass]
	public class TestImgDiff : IAmTest
	{
		const Activity Which = Activity.Encrypt;
		const int num = (int)Which;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void ImgDiff(int index)
		{
			using(var tempFile = Helpers.CreateTempPngFile())
			{
				var imgs = GetImageNames()[0];
				var inFiles = Helpers.InFile(imgs);
				string outFile = tempFile.TempFileName;
				string checkFile = Helpers.CheckFile(Which,imgs,index);
				var args = Helpers.Append(GetArgs(index),inFiles,outFile);

				Helpers.RunImageFunction(Which,args,outFile,checkFile);
			}
		}

		public string[] GetArgs(int index)
		{
			switch(index) {
			case 0: return new string[0];
			case 1: return new string[] { "-i" };
			case 2: return new string[] { "-o 1.0" };
			case 3: return new string[] { "-o 0.5" };
			}
			return null;
		}
		const int _CaseCount = 1;
		public int CaseCount { get { return _CaseCount; }}
		public FileSet Set { get { return FileSet.OneOne; }}

		public static IEnumerable<object[]> GetData()
		{
			for(int i=0; i<_CaseCount; i++) {
				yield return new object[] { i };
			}
		}

		public ITuple[] GetImageNames()
		{
			return new ITuple[] {
				("toes","toes-p"),
				("rock","rock-p"),
				("scorpius","scorpius-p"),
				("shack","shack-p")
			};
		}
	}
}
