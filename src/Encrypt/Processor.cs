using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageFunctions.Encrypt
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			Encryptor processor = new Encryptor();
			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				//copy section from frame
				var canvasRect = new Rectangle(0,0,rect.Width,rect.Height);
				var framePoint = new Point(rect.Left,rect.Top);
				canvas.Frames.RootFrame.BlitImage(frame,canvasRect,framePoint);

				//Encryption really wants to use streams
				var inStream = new PixelStream<TPixel>(canvas);
				inStream.PadToBlockSize = Encryptor.BlockSizeInBytes;
				var outStream = new PixelStream<TPixel>(canvas);
				outStream.PadToBlockSize = Encryptor.BlockSizeInBytes;

				using(inStream) using(outStream) {
					processor.TransformStream(O.DoDecryption,inStream,outStream,O.Password,progress);
				}

				//put processed section back
				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}
	}
}
