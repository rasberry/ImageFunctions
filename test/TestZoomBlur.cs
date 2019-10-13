using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestZoomBlur
	{
		// images used on wiki
		// zebra,boy,building,cats,cloud,cookie,creek

		const string name = "zebra";
		const Activity Which = Activity.ZoomBlur;
		const int num = (int)Which;

		[TestMethod]
		public void TestDefault()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-1.png");
			var args = new List<string>();
			Helpers.RunImageFunction(Which,args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_z3()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-2.png");
			var args = new List<string>{ "-z","3" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}
	}
}
