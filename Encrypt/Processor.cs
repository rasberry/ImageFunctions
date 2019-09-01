using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace ImageFunctions.Encrypt
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public bool DoDecryption { get; set; } = false;
		public byte[] Password { get; set; } = null;

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

				var inStream = new PixelStream<TPixel>(canvas);
				var outStream = new PixelStream<TPixel>(canvas);

				using(inStream) using(outStream) {
					processor.TransformStream(DoDecryption,inStream,outStream,Password);
				}

				//put processed section back
				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}
	}
}
