using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core.ColorSpace;

//No point in doing lazy since colorspace classes don't store anything they are cheaper than Lazy
public class Color3SpaceRegister : AbstractRegistrant<IColor3Space>
{
	public Color3SpaceRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal const string NS = "Color3Space";
	public override string Namespace { get { return NS; } }

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var reg = new Color3SpaceRegister(register);
		reg.Add("Cie1931", new ColorSpaceCie1931());
		reg.Add("Cie1960", new ColorSpaceCie1960());
		reg.Add("Cmy", new ColorSpaceCmy());
		reg.Add("Hsi", new ColorSpaceHsi());
		reg.Add("Hsl", new ColorSpaceHsl());
		reg.Add("Hsv", new ColorSpaceHsv());
		reg.Add("LinearRgb", new ColorSpsaceLinearRGB());
		reg.Add("Rgb", new ColorSpaceRGB());
		reg.Add("YCbCrBt202", new ColorSpaceYCbCrBt202());
		reg.Add("YCbCrBt601", new ColorSpaceYCbCrBt601());
		reg.Add("YCbCrBt709", new ColorSpaceYCbCrBt709());
		reg.Add("YCbCrJpeg", new ColorSpaceYCbCrJpeg());
		reg.Add("YCbCrSmpte240m", new ColorSpaceYCbCrSmpte240m());
		reg.Add("YDbDr", new ColorSpaceYDbDr());
		reg.Add("Yiq", new ColorSpaceYiq());
		reg.Add("YiqFcc", new ColorSpaceYiqFcc());
		reg.Add("YuvBT601", new ColorSpaceYuvBT601());
		reg.Add("YuvBT709", new ColorSpaceYuvBT709());
		reg.Default("Rgb");

		register.SetCustomHelpPrinter(NS,ColorSpaceHelpers.TryPrintColorSpace);
	}
}

public class Color4SpaceRegister : AbstractRegistrant<IColor4Space>
{
	public Color4SpaceRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal const string NS = "Color4Space";
	public override string Namespace { get { return NS; } }

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var reg = new Color4SpaceRegister(register);
		reg.Add("Cmyk", new ColorSpaceCmyk());
		reg.Default("Cmyk");

		register.SetCustomHelpPrinter(NS,ColorSpaceHelpers.TryPrintColorSpace);
	}
}
