using System;

namespace ImageFunctions.Engines
{
	public enum PickEngine
	{
		None = 0,
		ImageMagick = 1,
		SixLabors = 2
	}

	public static class Engine
	{
		public static PickEngine WhichEngine { get; set; }

		public static IFImageConfig GetConfig()
		{
			//TODO set via configuration
			WhichEngine = PickEngine.ImageMagick;

			switch(WhichEngine)
			{
			case PickEngine.ImageMagick:
				return new Engines.ImageMagick.IMImageConfig();
			case PickEngine.SixLabors:
				return new Engines.SixLabors.SLImageConfig();
			}
			throw new NotSupportedException($"Engine {WhichEngine} is not supported");
		}
	}
}
