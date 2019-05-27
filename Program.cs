using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;

namespace ImageFunctions
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1) {
				Log.Message("Usage: "+nameof(ImageFunctions)+" [options] (input image file) [output image file]");
				return;
			}

			string inputFileName = args[0];
			byte[] data = File.ReadAllBytes(inputFileName);
			var imgIn = Image.Load(data);

			var img = imgIn.Clone((ctx) => {
				ctx.ApplyProcessor(new PixelateDetails<Rgba32>());
			});


			//var img = new Image<Rgba32>(1024,1024);

			// try #1
			//int inCx = imgIn.Width / 2;
			//int inCy = imgIn.Width / 2;
			//for(int iy=0; iy<imgIn.Height; iy++) {
			//	for (int ix=0; ix<imgIn.Width; ix++) {
			//		var inRow = imgIn.GetPixelRowSpan(iy);
			//		double inOx = Math.Abs(ix); // - inCx;
			//		double inOy = Math.Abs(iy); // - inCy;
			//		double ox = 1023.0 - (1023.0/(inOx + 1.0));
			//		double oy = 1023.0 - (1023.0/(inOy + 1.0));
			//		// Log.Message("ox="+ox+" oy="+oy);
			//		var row = img.GetPixelRowSpan((int)oy);
			//		row[(int)ox] = inRow[ix];
			//	}
			//}

			//try #2
			//double den = Math.Log(Math.Pow(2.0,0.5)); //1-2 => 0-2
			//double scaleX = (double)imgIn.Width / img.Width;
			//double scaleY = (double)imgIn.Height / img.Height;
//
			//for(int iy=0; iy<imgIn.Height; iy++) {
			//	double rowY = (double)iy / imgIn.Height + 1;
			//	double mapY = Math.Log(rowY) / den;
			//	for (int ix=0; ix<imgIn.Width; ix++) {
			//		var inRow = imgIn.GetPixelRowSpan(iy);
			//		double rowX = (double)ix / imgIn.Width + 1;
			//		double mapX = Math.Log(rowX) / den;
//
			//		double ox = mapX / 2.0 * img.Width;
			//		double oy = mapY / 2.0 * img.Height;
			//		var row = img.GetPixelRowSpan((int)oy);
			//		row[(int)ox] = inRow[ix];
			//	}
			//}

			string outFile = "test-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png";
			using (var ofs = File.OpenWrite(outFile)) {
				img.SaveAsPng(ofs);
			}
		}
	}
}
