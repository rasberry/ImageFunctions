#if DEBUG
using System;
using ImageFunctions.Helpers;

namespace ImageFunctions.Playground
{
	public class Processor : AbstractProcessor
	{
		public override void Apply()
		{
			var Iis = Registry.GetImageEngine();
			var Idc = Registry.GetDrawEngine();
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
#endif