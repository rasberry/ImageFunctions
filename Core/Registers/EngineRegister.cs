using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class EngineRegister : AbstractRegistrant<Lazy<IImageEngine>>
{
	public EngineRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal override string Namespace { get { return "Engine"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var er = new EngineRegister(register);
		er.Add(ImageMagickString,new Lazy<IImageEngine>(() => new Engines.ImageMagickEngine()));
		er.Add(SixLaborsString,new Lazy<IImageEngine>(() => new Engines.SixLaborsEngine()));
	}

	internal const string ImageMagickString = "ImageMagick";
	internal const string SixLaborsString = "SixLabors";
}