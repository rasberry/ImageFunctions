using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public abstract class AbstractFunction : IFunction, IImageProcessor
	{
		public void Main()
		{
			using (var fs = File.Open(InImage,FileMode.Open,FileAccess.Read,FileShare.Read))
			using (var img = Image.Load<Rgba32>(fs))
			using (var proc = CreatePixelSpecificProcessor(img,Rect))
			{
				var absProc = proc as AbstractProcessor<Rgba32>;
				absProc.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
				absProc.Apply();
				ImageHelpers.SaveAsPng(OutImage,img);
			}
		}

		public Rectangle Rect { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }

		public abstract void Usage(StringBuilder sb);
		public abstract bool ParseArgs(string[] args);

		public abstract IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
			where TPixel : struct, IPixel<TPixel>;

		protected string InImage = null;
		protected string OutImage = null;

	}
}