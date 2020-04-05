using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;

namespace test
{
	[TestClass]
	public class TestOptionsHelpers
	{
		[DataTestMethod]
		[DataRow("1",1.0)]
		[DataRow("1%",0.01)]
		[DataRow("0.2",0.2)]
		[DataRow("0.2%",0.002)]
		[DataRow("1e+2",100.0)]
		[DataRow("1e+2%",1.0)]
		[DataRow("-99%",-0.99)]
		public void TestParseNumberPercent(string sval, double expected)
		{
			bool worked = OptionsHelpers.ParseNumberPercent(sval,out double check);
			Assert.IsTrue(worked);
			Assert.AreEqual(expected,check);
		}

		[DataTestMethod]
		[DataRow("a")]
		[DataRow("1.1.1")]
		[DataRow("-Infinity")]
		[DataRow("1e+1000")]
		public void TestParseBadNumberPercent(string sval)
		{
			bool worked = OptionsHelpers.ParseNumberPercent(sval,out double check);
			Assert.IsFalse(worked);
		}

		[DataTestMethod]
		[DataRow("#FFF","FFFFFFFF")]
		[DataRow("white","FFFFFFFF")]
		[DataRow("blue","0000FFFF")]
		[DataRow("FFF","FFFFFFFF")]
		[DataRow("C0C0C0","C0C0C0FF")]
		[DataRow("c0c0c0","C0C0C0FF")]
		[DataRow("#c0c0c0","C0C0C0FF")]
		[DataRow("#aabbcc00","AABBCC00")]
		public void TestTryParseColor(string color,string expected)
		{
			bool worked = OptionsHelpers.TryParseColor(color,out Color c);
			Assert.IsTrue(worked);
			Assert.AreEqual(expected,c.ToHex());
		}

		[DataTestMethod]
		[DataRow("#qq")]
		[DataRow("#FFFFFG")]
		[DataRow("#AABBCCDDEE")]
		[DataRow("unknown")]
		[DataRow("#white")]
		public void TestTryBadParseColor(string color)
		{
			bool worked = OptionsHelpers.TryParseColor(color,out Color c);
			Assert.IsFalse(worked);
		}

		[DataTestMethod]
		[DataRow("0.01",0.01)]
		[DataRow("1e-3",0.001)]
		[DataRow("12345.6",12345.6)]
		[DataRow("-1.6",-1.6)]
		public void TestTryParseDouble(string s, double d)
		{
			bool worked = OptionsHelpers.TryParse(s,out double val);
			Assert.IsTrue(worked);
			Assert.AreEqual(d,val);
		}

		[DataTestMethod]
		[DataRow("bad")]
		[DataRow("1.0a")]
		public void TestTryParseBadDouble(string s)
		{
			bool worked = OptionsHelpers.TryParse(s,out double val);
			Assert.IsFalse(worked);
		}

		[DataTestMethod]
		[DataRow("1",ImageFunctions.Activity.PixelateDetails)]
		[DataRow("PixelateDetails",ImageFunctions.Activity.PixelateDetails)]
		[DataRow("pixelatedetails",ImageFunctions.Activity.PixelateDetails)]
		public void TestTryParseEnum(string s, ImageFunctions.Activity a)
		{
			bool worked = OptionsHelpers.TryParse(s,out ImageFunctions.Activity val);
			Assert.IsTrue(worked);
			Assert.AreEqual(a,val);
		}

		[DataTestMethod]
		[DataRow("0,0,10,10",0,0,10,10)]
		[DataRow("10,10,20,20",10,10,20,20)]
		[DataRow("10x10,20x20",10,10,20,20)]
		[DataRow("10 10 20 20",10,10,20,20)]
		public void TestTryParseRect(string s, int x,int y,int w,int h)
		{
			bool worked = OptionsHelpers.TryParse(s,out SixLabors.Primitives.Rectangle val);
			Assert.IsTrue(worked);
			Assert.AreEqual(new SixLabors.Primitives.Rectangle(x,y,w,h),val);
		}

		[DataTestMethod]
		[DataRow("-1,-1,10,10")]
		[DataRow("10,10,-2,-2")]
		[DataRow("10-10-20-20")]
		[DataRow("0,0,0,0")]
		public void TestTryParseBadRect(string s)
		{
			bool worked = OptionsHelpers.TryParse(s,out SixLabors.Primitives.Rectangle val);
			Assert.IsFalse(worked);
		}

		public void TestTryParseString()
		{
			string arg = "opt1";
			bool worked = OptionsHelpers.TryParse(arg,out string val);
			Assert.IsTrue(worked);
			Assert.AreEqual(arg,val);
		}
	}
}
