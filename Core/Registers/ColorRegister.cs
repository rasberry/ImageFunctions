using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class ColorRegister : AbstractRegistrant<ColorRGBA>
{
	public ColorRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	public override string Namespace { get { return "Color"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var reg = new ColorRegister(register);
		var colorEnum = Enum.GetValues(typeof(System.Drawing.KnownColor));
		foreach(System.Drawing.KnownColor kc in colorEnum) {
			var sdColor = System.Drawing.Color.FromKnownColor(kc);
			bool good = sdColor.IsKnownColor && !sdColor.IsSystemColor;
			if (good) {
				var ifColor = ColorRGBA.FromRGBA255(sdColor.R, sdColor.G, sdColor.B, sdColor.A);
				reg.Add(sdColor.Name,ifColor);
			}
		}

		//add in special extras
		//reg.Add("RebeccaPurple",ColorRGBA.FromRGBA255(0xFF,0x66,0x33,0x99));
		//reg.Add("Transparent",new ColorRGBA(0.0, 0.0, 0.0, 0.0));
	}
}
