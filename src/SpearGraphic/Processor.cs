using System;
using System.Collections.Generic;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageFunctions.SpearGraphic
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var img = this.Source;
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
			}
		}
	}
}
