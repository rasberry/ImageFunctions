using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestPixelateDetails
	{
		// images used on wiki
		// boy, building, cats, cloud, cookie, creek, flower
		const string name = "boy";
		const Activity Which = Activity.PixelateDetails;
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
		public void Test_p()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-2.png");
			var args = new List<string>{ "-p" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_s3()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-3.png");
			var args = new List<string>{ "-s","3" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_r3()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-1-"+name+"-4.png");
			var args = new List<string>{ "-r","3" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}
	}
}
