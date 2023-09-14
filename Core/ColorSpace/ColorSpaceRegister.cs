using System.Reflection;
using System.Runtime.Intrinsics.X86;
using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core.ColorSpace;

//No point in doing lazy since colorspace classes don't store anything they are
// cheaper than Lazy
public class Color3SpaceRegister : AbstractRegistrant<IColor3Space>
{
	public Color3SpaceRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal override string Namespace { get { return "Color3Space"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var reg = new Color3SpaceRegister(register);
		reg.Add("Hsl"      ,new ColorSpaceHsl());
		reg.Add("Hsv"      ,new ColorSpaceHsv());
		reg.Add("Hsi"      ,new ColorSpaceHsi());
		reg.Add("Yiq"      ,new ColorSpaceYiq());
		reg.Add("YiqFcc"   ,new ColorSpaceYiqFcc());
		reg.Add("YuvBT601" ,new ColorSpaceYuvBT601());
		reg.Add("YuvBT709" ,new ColorSpaceYuvBT709());
		reg.Add("YDbDr"    ,new ColorSpaceYDbDr());
		reg.Add("YCbCrJpeg",new ColorSpaceYCbCrJpeg());
		reg.Add("Cie1931"  ,new ColorSpaceCie1931());
		reg.Add("Cie1960"  ,new ColorSpaceCie1960());
	}
}

public class Color4SpaceRegister : AbstractRegistrant<IColor4Space>
{
	public Color4SpaceRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal override string Namespace { get { return "Color4Space"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var reg = new Color4SpaceRegister(register);
		reg.Add("Cmyk", new ColorSpaceCmyk());
	}
}
