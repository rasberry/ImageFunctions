
using System;
using ImageFunctions.Helpers;
using ImageFunctions.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ImageFunctions;

namespace test
{
	[TestClass]
	public class TestSampleHelpers
	{
		IFImage _img = null;
		IFImage Image { get {
			if (_img == null) {
				string file = (string)Helpers.InFile(Tuple.Create("flower"))[0];
				var Iis = Engine.GetConfig();
				_img = Iis.LoadImage(file);
			}
			return _img;
		}}

		[TestMethod]
		public void Test()
		{
			var img = Image;
			var s = Registry.Map(Sampler.NearestNeighbor);
			var test = SampleHelpers.GetSample(img,s,10,10,null);
			var ecolor = new IFColor(0,0,0,0);
			Helpers.AssertAreSimilar(ecolor,test,1.0);
			//TODO test fails currently
		}
	}
}
