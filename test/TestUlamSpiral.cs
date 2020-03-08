using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SixLabors.Primitives;

namespace test
{
	[TestClass]
	public class TestUlamSpiral : IAmTestNoneOne
	{
		const Activity Which = Activity.UlamSpiral;

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
			case 1: return new string[] { "-c1","white","-c2","black" };
			case 2: return new string[] { "-m", "1" };
			case 3: return new string[] { "-m", "2" };
			case 4: return new string[] { "-6m" };
			case 5: return new string[] { "-f" };
			case 6: return new string[] { "-f","-s","2","-ds","20.0" };
			}
			return null;
		}
		const int _CaseCount = 7;
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
			return index == 0 ? "Default" : $"ulam";
		}

		public Rectangle? GetBounds(int index)
		{
			return new Rectangle(0,0,256,256);
		}
	}
}
