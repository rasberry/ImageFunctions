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
	public class TestGraphNet : IAmTestNoneOne
	{
		const Activity Which = Activity.GraphNet;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void GraphNet(int index)
		{
			Helpers.RunTestGenerator(Which,index,this);
		}

		public string[] GetArgs(int index)
		{
			switch(index) {
			case 0: return new string[] { "-rs","77" };
			case 1: return new string[] { "-rs","88","-p","0.1%" };
			case 2: return new string[] { "-rs","216","-b","256" };
			}
			return null;
		}
		const int _CaseCount = 3;
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
			return "gnet";
		}

		public Rectangle? GetBounds(int index)
		{
			return new Rectangle(0,0,256,256);
		}
	}
}
