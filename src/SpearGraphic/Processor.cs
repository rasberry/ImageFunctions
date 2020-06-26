using System;
using System.Collections.Generic;
using ImageFunctions.Helpers;

namespace ImageFunctions.SpearGraphic
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var img = Source;

			switch(O.Spear)
			{
			case Graphic.First_Twist1:
				First.Twist1(img,img.Width,img.Height); break;
			case Graphic.First_Twist2:
				First.Twist2(img,img.Width,img.Height); break;
			case Graphic.First_Twist3:
				First.Twist3(img,img.Width,img.Height); break;
			case Graphic.Second_Twist3a:
				Second.Twist3(img,img.Width,img.Height,0); break;
			case Graphic.Second_Twist3b:
				Second.Twist3(img,img.Width,img.Height,1); break;
			case Graphic.Second_Twist3c:
				Second.Twist3(img,img.Width,img.Height,2); break;
			case Graphic.Second_Twist4:
				Second.Twist4(img,img.Width,img.Height); break;
			case Graphic.Third:
				Third.Twist1(img,img.Width,img.Width,O.RandomSeed); break;
			case Graphic.Fourth:
				Fourth.Draw(img,img.Width,img.Height,O.RandomSeed); break;
			}
		}

		public override void Dispose() {}
	}

	#if false
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			//using the source image is a litle strange, but
			//SixLabors doesn't have a frame-specific processing context
			var img = this.Source;
			img.Mutate(op => op.BackgroundColor(O.BackgroundColor));

			switch(O.Spear)
			{
			case Graphic.First_Twist1:
				First<TPixel>.Twist1(frame,frame.Width,frame.Height); break;
			case Graphic.First_Twist2:
				First<TPixel>.Twist2(frame,frame.Width,frame.Height); break;
			case Graphic.First_Twist3:
				First<TPixel>.Twist3(frame,frame.Width,frame.Height); break;
			case Graphic.Second_Twist3a:
				Second<TPixel>.Twist3(img,img.Width,img.Height,0); break;
			case Graphic.Second_Twist3b:
				Second<TPixel>.Twist3(img,img.Width,img.Height,1); break;
			case Graphic.Second_Twist3c:
				Second<TPixel>.Twist3(img,img.Width,img.Height,2); break;
			case Graphic.Second_Twist4:
				Second<TPixel>.Twist4(img,img.Width,img.Height); break;
			case Graphic.Third:
				Third<TPixel>.Twist1(img,img.Width,img.Width,O.RandomSeed); break;
			case Graphic.Fourth:
				Fourth<TPixel>.Draw(img,img.Width,img.Height,O.RandomSeed); break;
			}
		}
	}
	#endif
}
