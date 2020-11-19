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
	public class TestMaze : IAmTestNoneOne
	{
		const Activity Which = Activity.Maze;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void Maze(int index)
		{
			Helpers.RunTestGenerator(Which,index,this);
		}

		public string[] GetArgs(int index)
		{
			switch(index) {
			case 0: return new string[] { "1","-rs","5003" };
			case 1: return new string[] { "2","-rs","5009","-cc","tan","-wc","red" };
			case 2: return new string[] { "3","-rs","5011" };
			case 3: return new string[] { "4","-rs","5021" };
			case 4: return new string[] { "5","-rs","5023" };
			case 5: return new string[] { "6","-rs","5039" };
			case 6: return new string[] { "7","-rs","5051" };
			case 7: return new string[] { "8","-rs","5059" };
			case 8: return new string[] { "9","-rs","5077" };
			case 9: return new string[] { "10","-rs","5081" };
			}
			return null;
		}

		const int _CaseCount = 10;
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
			return "maze";
		}

		public Rectangle? GetBounds(int index)
		{
			return new Rectangle(0,0,256,256);
		}
	}
}
