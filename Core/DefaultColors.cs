#if false
using System.Drawing;
using ImageFunctions.Core.ColorSpace;

namespace ImageFunctions.Core;

static class DefaultColors : IRegistrant<ColorRGBA>
{
	public static void RegisterColors(IRegister register)
	{
		var colorEnum = Enum.GetValues(typeof(KnownColor));
		foreach(KnownColor kc in colorEnum) {
			var sdColor = Color.FromKnownColor(kc);
			bool good = sdColor.IsKnownColor && !sdColor.IsSystemColor;
			if (good) {
				var ifColor = ColorRGBA.FromRGBA255(sdColor.R, sdColor.G, sdColor.B, sdColor.A);
				register.AddColor(sdColor.Name,ifColor);
			}
		}

		//add in special extra
		register.AddColor("RebeccaPurple",ColorRGBA.FromRGBA255(0xFF,0x66,0x33,0x99));
		register.AddColor("Transparent",new ColorRGBA(0.0, 0.0, 0.0, 0.0));
	}
}
#endif