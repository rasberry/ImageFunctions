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

}
