using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestAreaSmoother
	{
		// images used on wiki
		// rock-p,scorpius-p,shack-p,shell-p,skull-p,spider-p,toes-p

		const string name = "rock-p";
		const Activity Which = Activity.AreaSmoother;
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
		public void Test_t2()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-2.png");
			var args = new List<string>{ "-t","2" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_t10()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-3.png");
			var args = new List<string>{ "-t","10" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_metric1()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-4.png");
			var args = new List<string>{ "--metric","1" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_sampler11()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-5.png");
			var args = new List<string>{ "--sampler","11" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}

	}
}
