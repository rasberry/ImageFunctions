using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace test
{
	[TestClass]
	public class TestDeform : IAmTest
	{
		const Activity Which = Activity.Deform;
		const int num = (int)Which;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void Deform(int index)
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
			case 1: return new string[] { "-e","2.5" };
			case 2: return new string[] { "-m","2" };
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

		public ITuple[] GetImageNames()
		{
			var list = new string[] { "road","rock","scorpius","shack","shell","skull","spider" };
			return Helpers.Tupleify(list);
		}
	}
}
