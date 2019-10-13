using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions;
using System.IO;
using System;

namespace test
{
	[TestClass]
	public class TestPixelateDetails
	{
		[TestMethod]
		public void Test1()
		{
			// | Image   | Default | -p | -s 3 | -r 3 |
			string pr = Helpers.ProjectRoot;
			string temp = Path.GetTempFileName();

			IFunction func = Registry.Map(Activity.PixelateDetails);
			bool worked = func.ParseArgs(new string[] {
				Path.Combine(pr,"wiki","img","boy.png",temp)
			});
			Assert.IsTrue(worked);
		}
	}
}
