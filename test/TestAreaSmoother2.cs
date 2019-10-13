using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestAreaSmoother2
	{
		// images used on wiki
		// rock-p,scorpius-p,shack-p,shell-p,skull-p,spider-p,toes-p

		const string name = "scorpius-p";
		const Activity Which = Activity.AreaSmoother2;
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
		public void Test_H()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-2.png");
			var args = new List<string>{ "-H" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_V()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-3.png");
			var args = new List<string>{ "-V" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}
	}
}
