using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace test
{
	[TestClass]
	public class TestAreaSmoother : IAmTest
	{
		const Activity Which = Activity.AreaSmoother;
		const int num = (int)Which;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void AreaSmoother(int index)
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
				case 1: return new string[] { "-t","2" };
				case 2: return new string[] { "-t","10" };
				case 3: return new string[] { "--metric","1" };
				case 4: return new string[] { "--sampler","11" };
			}
			return null;
		}
		const int _CaseCount = 5;
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
			var list = new string[] { "rock-p","scorpius-p","shack-p","shell-p","skull-p","spider-p","toes-p" };
			return Helpers.Tupleify(list);
		}
	}
}
