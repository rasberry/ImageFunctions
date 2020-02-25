using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public abstract class AbstractProcessor<TPixel> : IImageProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public void Apply()
		{
			var config = Source.GetConfiguration();
			if (MaxDegreeOfParallelism.HasValue) {
				config.MaxDegreeOfParallelism = MaxDegreeOfParallelism.Value;
			}
			foreach (ImageFrame<TPixel> sourceFrame in Source.Frames) {
				var rect = Bounds;
				if (rect.IsEmpty) { rect = sourceFrame.Bounds(); }
				rect.X = 0; rect.Y = 0; //ignore passed in x,y so we don't get index out of bound errors

				this.Apply(sourceFrame, rect, config);
			}
		}

		protected abstract void Apply(ImageFrame<TPixel> frame, Rectangle rectangle, Configuration config);

		public virtual void Dispose() {}
		public Image<TPixel> Source { get; set; }
		public Rectangle Bounds { private get; set; } //don't let subclasses read this - should be relying on the one passed in
		public int? MaxDegreeOfParallelism { get; set; }
	}
}
