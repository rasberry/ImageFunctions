using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestDerivatives
	{
		// images used on wiki
		// fractal,handle,harddrive,lego,pool,rainbow,road

		const string name = "fractal";

		[TestMethod]
		public void TestDefault()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-2-"+name+"-1.png");
			var args = new List<string>();
			Helpers.RunImageFunction(Activity.Derivatives,args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_p()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-2-"+name+"-2.png");
			var args = new List<string>{ "-g" };
			Helpers.RunImageFunction(Activity.Derivatives, args, inFile, checkFile);
		}

		[TestMethod]
		public void Test_s3()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-2-"+name+"-3.png");
			var args = new List<string>{ "-a" };
			Helpers.RunImageFunction(Activity.Derivatives, args, inFile, checkFile);
		}
	}
}
