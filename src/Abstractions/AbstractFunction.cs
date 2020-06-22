using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions
{
	#if false
	public abstract class AbstractFunction : IFunction, IImageProcessor
	{
		public abstract void Main();

		public void Main<TPixel>()
			where TPixel : struct, IPixel<TPixel>
		{
			if (InImage == null) {
				var rect = Bounds;
				using (var img = new Image<TPixel>(rect.Width,rect.Height)) {
					DoProcessing(img);
				}
			}
			else {
				using (var fs = File.Open(InImage,FileMode.Open,FileAccess.Read,FileShare.Read))
				using (var img = Image.Load<TPixel>(fs)) {
					DoProcessing(img);
				}
			}
		}

		void DoProcessing<TPixel>(Image<TPixel> img)
			where TPixel : struct, IPixel<TPixel>
		{
			using (var proc = CreatePixelSpecificProcessor<TPixel>(img,Bounds))
			{
				var absProc = proc as AbstractProcessor<TPixel>;
				absProc.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
				absProc.Apply();
				ImageHelpers.SaveAsPng(OutImage,img);
			}
		}

		public Rectangle Bounds { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }

		public abstract void Usage(StringBuilder sb);
		public abstract bool ParseArgs(string[] args);

		public abstract IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
			where TPixel : struct, IPixel<TPixel>;

		protected string InImage = null;
		protected string OutImage = null;
	}
	#endif
}