using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageFunctions
{
	public static class Helpers
	{
		public static string CreateOutputFileName(string input)
		{
			//string ex = Path.GetExtension(input);
			string name = Path.GetFileNameWithoutExtension(input);
			string outFile = name+"-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png";
			return outFile;
		}

		public static void SaveAsPng<TPixel>(string fileName, Image<TPixel> image) where TPixel : struct, IPixel<TPixel>
		{
			PngEncoder encoder = new PngEncoder();
			encoder.CompressionLevel = 9;
			image.Save(fileName,encoder);
		}
	}
}
