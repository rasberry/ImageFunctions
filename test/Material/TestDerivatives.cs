using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace test
{
	[TestClass]
	public class TestDerivatives : IAmTestSomeOne
	{
		const Activity Which = Activity.Derivatives;
		const int num = (int)Which;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void Derivatives(int index)
		{
			Helpers.RunTestWithInputFiles(Which,index,this);
		}

		public string[] GetArgs(int index)
		{
			switch(index) {
			case 0: return new string[0];
			case 1: return new string[] { "-g" };
			case 2: return new string[] { "-a" };
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
			var list = new string[] { "fractal","handle","harddrive","lego","pool","rainbow","road" };
			return Helpers.Tupleify(list);
		}
	}
}
