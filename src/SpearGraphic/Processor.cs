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
			switch(O.Spear)
			{
			case Graphic.First_Twist1:
				First<TPixel>.Twist1(frame,frame.Width,frame.Height); break;
			case Graphic.First_Twist2:
				First<TPixel>.Twist2(frame,frame.Width,frame.Height); break;
			case Graphic.First_Twist3:
				First<TPixel>.Twist3(frame,frame.Width,frame.Height); break;
			}
		}
	}
}
