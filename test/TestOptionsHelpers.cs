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
	}
}
