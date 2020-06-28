using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions.Helpers;
using System.Collections.Generic;
using System.Drawing;

namespace test
{
	[TestClass]
	public class TestColorHelpers
	{
		[DataTestMethod]
		[DataRow("#AABBCCFF",0xAA,0xBB,0xCC,0xFF)]
		[DataRow("#00000000",0x00,0x00,0x00,0x00)]
		[DataRow("#FFFFFF00",0xFF,0xFF,0xFF,0x00)]
		[DataRow("#FFFFFFFF",0xFF,0xFF,0xFF,0xFF)]
		public void TestToHex(string hex,int r,int g,int b,int a)
		{
			var c = Color.FromArgb(a,r,g,b);
			string toHex = c.ToHex();
			Assert.AreEqual(hex,toHex);
		}

		[DataTestMethod]
		[DataRow("#AABBCCFF",0xAA,0xBB,0xCC,0xFF)]
		[DataRow("#00000000",0x00,0x00,0x00,0x00)]
		[DataRow("#FFFFFF00",0xFF,0xFF,0xFF,0x00)]
		[DataRow("#FFFFFFFF",0xFF,0xFF,0xFF,0xFF)]
		[DataRow("#ABCF"    ,0xAA,0xBB,0xCC,0xFF)]
		[DataRow("#ABC"     ,0xAA,0xBB,0xCC,0xFF)]
		[DataRow("#000"     ,0x00,0x00,0x00,0xFF)]
		[DataRow("#0000"    ,0x00,0x00,0x00,0x00)]
		public void TestFromHex(string hex,int r,int g,int b,int a)
		{
			var c = Color.FromArgb(a,r,g,b);
			var test = ColorHelpers.FromHex(hex);
			Assert.AreEqual(c,test);
		}

	}
}