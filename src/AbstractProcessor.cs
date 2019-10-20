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
				var rect = SourceRectangle;
				if (rect.IsEmpty) { rect = sourceFrame.Bounds(); }

				this.Apply(sourceFrame, rect, config);
			}
		}

		protected abstract void Apply(ImageFrame<TPixel> frame, Rectangle rectangle, Configuration config);

		public virtual void Dispose() {}
		public Image<TPixel> Source { get; set; }
		public Rectangle SourceRectangle { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }
	}
}
