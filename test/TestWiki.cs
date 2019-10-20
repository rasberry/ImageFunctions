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
			//var sb = new StringBuilder();
			//foreach(var kvp in context.Properties) {
			//	sb.AppendLine(kvp.Key+":"+kvp.Value);
			//}
			//throw new Exception("props = "+sb.ToString());
			//bool buildFlag = context.Properties.TryGetValue("buildWiki", out object _);

			string val = Environment.GetEnvironmentVariable("BUILDWIKI");
			bool buildFlag = val == "1";
			if (buildFlag) {
				Materials.BuildWiki();
			}

			Assert.IsTrue(true);
		}
	}
}