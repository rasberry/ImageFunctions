using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestAreaSmoother2 : IAmTest
	{
		const Activity Which = Activity.AreaSmoother2;
		const int num = (int)Which;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void AreaSmoother2(int index)
		{
			using(var tempFile = Helpers.CreateTempPngFile())
			{
				string img = GetImageNames()[0];
				string inFile = Helpers.InFile(img);
				string outFile = tempFile.TempFileName;
				string checkFile = Helpers.CheckFile(Which,img,index);
				var args = Helpers.Append(GetArgs(index),inFile,outFile);

				Helpers.RunImageFunction(Which,args,outFile,checkFile);
			}
		}

		public string[] GetArgs(int index)
		{
			switch(index) {
			case 0: return new string[0];
			case 1: return new string[] { "-H" };
			case 2: return new string[] { "-V" };
			}
			return null;
		}
		const int _CaseCount = 3;
		public int CaseCount { get { return _CaseCount; }}
		public FileSet Set { get { return FileSet.OneOne; }}

		public static IEnumerable<object[]> GetData()
		{
			for(int i=0; i<_CaseCount; i++) {
				yield return new object[] { i };
			}
		}

		public string[] GetImageNames()
		{
			return new string[] { "rock-p","scorpius-p","shack-p","shell-p","skull-p","spider-p","toes-p" };
		}
	}
}
