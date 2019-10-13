using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestSwirl
	{
		// images used on wiki
		// flower,fractal,handle,harddrive,lego,pool,rainbow

		const string name = "flower";
		const Activity Which = Activity.Swirl;
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
		public void Test_rp50()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-2.png");
			var args = new List<string>{ "-rp","50%" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_s2()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-3.png");
			var args = new List<string>{ "-s","2" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_ccw()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-4.png");
			var args = new List<string>{ "-ccw" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}
	}
}
