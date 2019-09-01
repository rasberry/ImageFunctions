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
		public byte[] IVBytes { get; set; } = null;
		public byte[] SaltBytes { get; set; } = null;
		public int? PasswordIterations = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			Encryptor processor = new Encryptor();
			if (IVBytes != null) { processor.IVBytes = IVBytes; }
			if (SaltBytes != null) { processor.SaltBytes = SaltBytes; }
			if (PasswordIterations != null) { processor.PasswordIterations = PasswordIterations.Value; }

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
					processor.TransformStream(DoDecryption,inStream,outStream,Password,progress);
				}

				//put processed section back
				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}
	}
}
