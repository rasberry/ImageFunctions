using System;
using System.Text;

namespace ImageFunctions
{
	public static class Registry
	{
		public static IFunction Map(Action action)
		{
			switch(action)
			{
			default:
			case Action.None: return null;
			case Action.PixelateDetails: return new PixelateDetails.Function();
			case Action.Derivatives: return new Derivatives.Function();
			case Action.AreaSmoother: return new AreaSmoother.Function();
			case Action.AreaSmoother2: return new AreaSmoother2.Function();
			}
		}
	}
}
