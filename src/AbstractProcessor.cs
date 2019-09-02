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
		public void Apply(Image<TPixel> source, Rectangle sourceRectangle)
		{
			Configuration config = source.GetConfiguration();
			foreach (ImageFrame<TPixel> sourceFrame in source.Frames) {
				this.Apply(sourceFrame, sourceRectangle, config);
			}
		}

		protected abstract void Apply(ImageFrame<TPixel> frame, Rectangle rectangle, Configuration config);
	}
}




