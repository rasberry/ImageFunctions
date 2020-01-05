using System;
using System.Runtime.CompilerServices;
using ImageFunctions.AllColors;
using ImageFunctions.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.Primitives;
using System.IO;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestSpearGraphic : IAmTestNoneOne
	{
		const ImageFunctions.Activity Which = ImageFunctions.Activity.SpearGraphic;

		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void SpearGraphic(int index)
		{
			Helpers.RunTestGenerator(Which, index, this);
		}

		public static IEnumerable<object[]> GetData()
		{
			for(int i=0; i<_CaseCount; i++) {
				yield return new object[] { i };
			}
		}
		const int _CaseCount = 9;
		public int CaseCount { get { return _CaseCount; }}
		public FileSet Set { get { return  FileSet.NoneOne; }}

		public string[] GetArgs(int index)
		{
			switch(index)
			{
			case 0: return new string[] { "-g","First_Twist1"};
			case 1: return new string[] { "-g","First_Twist2"};
			case 2: return new string[] { "-g","First_Twist3"};
			case 3: return new string[] { "-g","Second_Twist3a"};
			case 4: return new string[] { "-g","Second_Twist3b"};
			case 5: return new string[] { "-g","Second_Twist3c"};
			case 6: return new string[] { "-g","Second_Twist4"};
			case 7: return new string[] { "-g","Third"};
			case 8: return new string[] { "-g","Fourth"};
			}
			return null;
		}

		public string GetOutName(int index)
		{
			var args = GetArgs(index);
			string name = args[1];
			return name;
		}

		public Rectangle? GetBounds(int index)
		{
			return new Rectangle(0,0,256,256);
		}
	}
}