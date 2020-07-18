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
		//TODO set via configuration
		public static PickEngine WhichEngine { get; set; } = PickEngine.SixLabors;

		public static IImageConfig GetConfig()
		{
			switch(WhichEngine)
			{
			case PickEngine.ImageMagick:
				return new Engines.ImageMagick.IMImageConfig();
			case PickEngine.SixLabors:
				return new Engines.SixLabors.SLImageConfig();
			}
			throw new NotSupportedException($"Engine {WhichEngine} is not supported");
		}

		public static IDrawConfig GetDrawable()
		{
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
