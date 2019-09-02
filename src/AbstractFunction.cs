using System;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public abstract class AbstractFunction : IFunction
	{
		public void Main()
		{
			using (var img = Image.Load(InImage))
			{
				img.SetMaxDegreeOfParallelism(MaxDegreeOfParallelism);
				img.Mutate(Process);
				ImageHelpers.SaveAsPng(OutImage,img);
			}
		}

		public Rectangle Rect { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }

		public abstract void Usage(StringBuilder sb);
		public abstract bool ParseArgs(string[] args);
		protected abstract void Process(IImageProcessingContext<Rgba32> ctx);

		protected string InImage = null;
		protected string OutImage = null;

	}
}