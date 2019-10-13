using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestPixelRules
	{
		// images used on wiki
		// boy,building,cats,cloud,cookie,creek,flower

		const string name = "building";
		const Activity Which = Activity.PixelRules;
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
		public void Test_m2()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-2.png");
			var args = new List<string>{ "-m","2" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_m3()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-3.png");
			var args = new List<string>{ "-m","3" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_n10()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-4.png");
			var args = new List<string>{ "-n","10" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}
	}
}
