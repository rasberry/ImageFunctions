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
		[DataRow("AABBCCFF" ,0xAA,0xBB,0xCC,0xFF)]
		[DataRow("ABCF"     ,0xAA,0xBB,0xCC,0xFF)]
		public void TestFromHex(string hex,int r,int g,int b,int a)
		{
			var c = Color.FromArgb(a,r,g,b);
			var test = ColorHelpers.FromHex(hex);
			AssertAreEqual(c,test);
		}

		[DataTestMethod]
		[DataRow("#AABBCCFG",typeof(System.FormatException))]
		[DataRow("Badness",typeof(System.ArgumentException))]
		public void TestFromHexBad(string hex, Type ext)
		{
			try {
				var text = ColorHelpers.FromHex(hex);
			}
			catch(Exception e) {
				Assert.AreEqual(ext,e.GetType());
			}
		}

		[DataTestMethod]
		[DataRow("blue"       ,0x00,0x00,0xFF,0xFF)]
		[DataRow("transparent",0xFF,0xFF,0xFF,0x00)]
		[DataRow("black"      ,0x00,0x00,0x00,0xFF)]
		[DataRow("green"      ,0x00,0x80,0x00,0xFF)]
		public void TestFromName(string name, int r,int g,int b,int a)
		{
			var c = Color.FromArgb(a,r,g,b);
			var test = ColorHelpers.FromName(name);
			AssertAreEqual(c,test.Value);
		}

		static void AssertAreEqual(Color exp, Color act)
		{
			//normalize the colors - comparing the same color created
			// from a name vs with FromArgb won't trigger a match
			Color c1 = Color.FromArgb(exp.A,exp.R,exp.G,exp.B);
			Color c2 = Color.FromArgb(act.A,act.R,act.G,act.B);
			Assert.AreEqual(c1,c2);
		}
	}
}