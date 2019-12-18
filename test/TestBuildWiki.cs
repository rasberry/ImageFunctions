using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace test
{
	[TestClass]
	public class TestWiki
	{
		[TestMethod]
		public void TestBuildWiki()
		{
			string val = Environment.GetEnvironmentVariable("BUILDWIKI");
			bool buildFlag = val == "1";
			if (buildFlag) {
				Materials.BuildWiki();
			}

			Assert.IsTrue(true);
		}
	}
}