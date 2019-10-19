using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestEncrypt : IAmTest
	{
		const Activity Which = Activity.Encrypt;
		const int num = (int)Which;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void Encrypt(int index)
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
			case 0: return new string[] { "-p", "1234" };
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

		public string[] GetImageNames()
		{
			return new string[] { "toes","zebra" };
		}
	}
}
