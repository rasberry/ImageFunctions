
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
				// ImageFunctions/wiki/img/flower.png
				string file = (string)Helpers.InFile(Tuple.Create("flower"))[0];
				var Iis = Engine.GetConfig();
				_img = Iis.LoadImage(file);
			}
			return _img;
		}}

		[DataTestMethod]
		[DataRow(Sampler.NearestNeighbor  ,0.001,1.0)]
		[DataRow(Sampler.Bicubic          ,0.010,1.5)]
		[DataRow(Sampler.Box              ,0.001,1.5)]
		[DataRow(Sampler.CatmullRom       ,0.010,1.5)]
		[DataRow(Sampler.Hermite          ,0.020,1.5)]
		[DataRow(Sampler.Lanczos2         ,0.010,1.5)]
		[DataRow(Sampler.Lanczos3         ,0.020,1.5)]
		[DataRow(Sampler.Lanczos5         ,0.020,1.5)]
		[DataRow(Sampler.Lanczos8         ,0.020,1.5)]
		[DataRow(Sampler.MitchellNetravali,0.020,1.5)]
		[DataRow(Sampler.Robidoux         ,0.020,1.5)]
		[DataRow(Sampler.RobidouxSharp    ,0.020,1.5)]
		[DataRow(Sampler.Spline           ,0.050,1.5)]
		[DataRow(Sampler.Triangle         ,0.030,1.5)]
		[DataRow(Sampler.Welch            ,0.030,1.5)]
		public void TestSamplers(Sampler sampler, double acc, double scale)
		{
			var img = Image;
			var s = Registry.Map(sampler);
			var test = SampleHelpers.GetSample(img,s,10,10,scale);
			var ecolor = new IFColor(0.522,0.439,0.396,1.0);
			Helpers.AssertAreSimilar(ecolor,test,acc);
		}

		[DataTestMethod]
		[DataRow(Sampler.NearestNeighbor  ,0.001,1.0)]
		[DataRow(Sampler.Bicubic          ,0.010,1.5)]
		[DataRow(Sampler.Box              ,0.001,1.5)]
		[DataRow(Sampler.CatmullRom       ,0.010,1.5)]
		[DataRow(Sampler.Hermite          ,0.020,1.5)]
		[DataRow(Sampler.Lanczos2         ,0.010,1.5)]
		[DataRow(Sampler.Lanczos3         ,0.020,1.5)]
		[DataRow(Sampler.Lanczos5         ,0.020,1.5)]
		[DataRow(Sampler.Lanczos8         ,0.020,1.5)]
		[DataRow(Sampler.MitchellNetravali,0.020,1.5)]
		[DataRow(Sampler.Robidoux         ,0.020,1.5)]
		[DataRow(Sampler.RobidouxSharp    ,0.020,1.5)]
		[DataRow(Sampler.Spline           ,0.050,1.5)]
		[DataRow(Sampler.Triangle         ,0.030,1.5)]
		[DataRow(Sampler.Welch            ,0.030,1.5)]
		public void TestSamplersEdge(Sampler sampler, double acc, double scale)
		{
			var img = Image;
			var s = Registry.Map(sampler);
			var test = SampleHelpers.GetSample(img,s,0,0,scale);
			var ecolor = new IFColor(0.498,0.447,0.412,1.0);
			Helpers.AssertAreSimilar(ecolor,test,acc);
		}

	}
}
