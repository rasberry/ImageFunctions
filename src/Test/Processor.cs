using System;
using ImageFunctions.Helpers;

namespace ImageFunctions.Test
{
	public class Processor : AbstractProcessor
	{
		public override void Apply()
		{
			var Iis = Engines.Engine.GetConfig();
			var Idc = Engines.Engine.GetDrawable();
			Random r = new Random();

			for(int i=0; i<10; i++) {
				int x0 = r.Next(Source.Width);
				int y0 = r.Next(Source.Height);
				int x1 = r.Next(Source.Width);
				int y1 = r.Next(Source.Height);

				Idc.DrawLine(Source,ColorHelpers.IndianRed,new PointD(x0,y0), new PointD(x1,y1),10);
			}
		}

		public override void Dispose()
		{
		}
	}
}