using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace test
{
	[TestClass]
	public class TestTurmites : IAmTestNoneOne
	{
		const Activity Which = Activity.Turmites;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void UlamSpiral(int index)
		{
			Helpers.RunTestGenerator(Which,index,this);
		}

		public string[] GetArgs(int index)
		{
			switch(index) {
			case 0: return new string[0];
			case 1: return new string[] { "-p", "LRRL" };
			case 2: return new string[] { "-p", "LRRL", "-i", "1e+7" };
			case 3: return new string[] { "-p", "LRRL", "-e", "reflect", "-i", "1e+7" };
			}
			return null;
		}
		const int _CaseCount = 4;
		public int CaseCount { get { return _CaseCount; }}
		public FileSet Set { get { return FileSet.NoneOne; }}

		public static IEnumerable<object[]> GetData()
		{
			for(int i=0; i<_CaseCount; i++) {
				yield return new object[] { i };
			}
		}

		public string GetOutName(int index)
		{
			return index == 0 ? "Default" : $"turmites";
		}

		public Rectangle? GetBounds(int index)
		{
			return new Rectangle(0,0,256,256);
		}
	}

}