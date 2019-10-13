using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestEncrypt
	{
		// images used on wiki
		// toes,zebra

		const string name = "toes";
		const Activity Which = Activity.Encrypt;
		const int num = (int)Which;

		[TestMethod]
		public void Test_p1234()
		{
			string inFile = Path.Combine(Helpers.ImgRoot,name + ".png");
			string checkFile = Path.Combine(Helpers.ImgRoot,"img-"+num+"-"+name+"-2.png");
			var args = new List<string>{ "-p","1234" };
			Helpers.RunImageFunction(Which, args, inFile, checkFile);
		}
	}
}
