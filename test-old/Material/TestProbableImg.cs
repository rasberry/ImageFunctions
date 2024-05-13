using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace test
{
	[TestClass]
	public class TestProbableImg : IAmTestSomeOne
	{
		const Activity Which = Activity.ProbableImg;
		const int num = (int)Which;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void ProbableImg(int index)
		{
			Helpers.RunTestWithInputFiles(Which,index,this);
		}

		public string[] GetArgs(int index)
		{
			switch(index) {
			case 0: return new string[] { "-rs","321", };
			case 1: return new string[] { "-rs","321","-n","5"};
			case 2: return new string[] { "-rs","321","-pp","50%","50%" };
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
			var list = new string[] { "boy","cats","cookie","flower","harddrive","shack" };
			return Helpers.Tupleify(list);
		}
	}
}
