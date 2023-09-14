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

#if false
public static class ColorSpaceExtensions
{
	public static void RegisterAllColorSpaces(this IRegister reg)
	{
		//TODO replace this with a Generator
		var a = Assembly.GetExecutingAssembly();
		foreach(var t in a.GetTypes()) {
			if (t is IColor3Space) {
				AddColor3Space(reg,t.FullName,() => (IColor3Space)Activator.CreateInstance(t));
			}
			else if (t is IColor4Space) {
				AddColor4Space(reg,t.FullName,() => (IColor4Space)Activator.CreateInstance(t));
			}
		}
	}

	const string Prefix3 = "Color3Space.";
	public static void AddColor3Space(this IRegister reg, string name, Func<IColor3Space> color) {
		reg.Add(Prefix3, name, color);
	}
	public static Func<IColor3Space> GetColor3Space(this IRegister reg, string name) {
		return reg.Get<Func<IColor3Space>>(Prefix3,name);
	}
	public static bool TryGetColor3Space(this IRegister reg, string name, out Func<IColor3Space> color) {
		return reg.Try(Prefix3, name, out color);
	}
	public static IEnumerable<string> GetAllColor3Space(this IRegister reg) {
		return reg.All<Func<IColor3Space>>(Prefix3);
	}

	const string Prefix4 = "Color4Space.";
	public static void AddColor4Space(this IRegister reg, string name, Func<IColor4Space> color) {
		reg.Add(Prefix4, name, color);
	}
	public static Func<IColor4Space> GetColor4Space(this IRegister reg, string name) {
		return reg.Get<Func<IColor4Space>>(Prefix4,name);
	}
	public static bool TryGetColor4Space(this IRegister reg, string name, out Func<IColor4Space> color) {
		return reg.Try(Prefix4, name, out color);
	}
	public static IEnumerable<string> GetAllColor4Space(this IRegister reg) {
		return reg.All<Func<IColor4Space>>(Prefix4);
	}
}
#endif