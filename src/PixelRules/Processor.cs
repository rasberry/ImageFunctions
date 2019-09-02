using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageFunctions.PixelRules
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Function.Mode WhichMode = Function.Mode.None;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				MoreHelpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					TPixel nc = RunRule(frame,x,y);
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				},progress);

				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}

		TPixel RunRule(ImageFrame<TPixel> frame,int x, int y)
		{
			return default(TPixel);
		}
	}
}
