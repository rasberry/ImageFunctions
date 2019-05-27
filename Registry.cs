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
			}
		}
	}
}
