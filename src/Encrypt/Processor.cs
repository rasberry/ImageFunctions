using System;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.Encrypt
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var frame = Source;
			var rect = Bounds;
			var Iis = Engines.Engine.GetConfig();

			Encryptor processor = new Encryptor();
			using (var progress = new ProgressBar())
			using (var canvas = Iis.NewImage(rect.Width,rect.Height))
			{
				//copy section from frame
				var canvasRect = new Rectangle(0,0,rect.Width,rect.Height);
				var framePoint = new Point(rect.Left,rect.Top);
				canvas.BlitImage(frame,canvasRect,framePoint);

				//Encryption really wants to use streams
				var inStream = new PixelStream(canvas);
				inStream.PadToBlockSize = Encryptor.BlockSizeInBytes;
				var outStream = new PixelStream(canvas);
				outStream.PadToBlockSize = Encryptor.BlockSizeInBytes;

				using(inStream) using(outStream) {
					processor.TransformStream(O.DoDecryption,inStream,outStream,O.Password,progress);
				}

				//put processed section back
				frame.BlitImage(canvas,rect);
			}
		}

		public override void Dispose() {}
	}
}
