using System;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;

namespace ImageFunctions.Helpers
{
	public static class ColorHelpers
	{
		// Colors used directly in the code
		public static IColor Black       { get { return RGB(0x00,0x00,0x00); }}
		public static IColor IndianRed   { get { return RGB(0xCD,0x5C,0x5C); }}
		public static IColor LimeGreen   { get { return RGB(0x32,0xCD,0x32); }}
		public static IColor Magenta     { get { return RGB(0xFF,0x00,0xFF); }}
		public static IColor Transparent { get { return RGB(0x00,0x00,0x00,0x00); }}
		public static IColor White       { get { return RGB(0xFF,0xFF,0xFF); }}

		static IColor RGB(int r,int g,int b,int a = 255)
		{
			return new IColor(r/255.0,g/255.0,b/255.0,a/255.0);
		}

		public static string ToHex(this IColor c)
		{
			var rgba = ImageHelpers.NativeToRgba(c);
			return ToHex(rgba);
		}
		public static string ToHex(this Color c)
		{
			uint argb = (uint)c.ToArgb();
			uint rgba = (argb << 8) | (argb >> 24); //Rotate left. must be done with uint or we get -1 (0xFF...)
			var s = $"{rgba:X8}";
			return s;
		}

		public static IColor FromHexNative(string s)
		{
			var c = FromHex(s);
			return ImageHelpers.RgbaToNative(c);
		}

		public static Color FromHex(string s)
		{
			if (s[0] == '#') {
				s = s.Substring(1);
			}
			int len = s.Length;
			if (len != 8 && len != 6 && len != 4 && len != 3) {
				throw new ArgumentException("Hex string length must be one of 3,4,6,8");
			}

			int scale = len == 8 || len == 6 ? 2 : 1;
			bool hasAlpha = len == 8 || len == 4;
			int scaleHex = scale == 1 ? 17 : 1; //scaling by 17 'doubles' the digit A -> AA, B -> BB

			string rr = s.Substring(0 * scale,scale);
			string gg = s.Substring(1 * scale,scale);
			string bb = s.Substring(2 * scale,scale);
			string aa = hasAlpha ? s.Substring(3 * scale,scale) : new String('F',scale);

			int cr = ParseHex(rr) * scaleHex;
			int cg = ParseHex(gg) * scaleHex;
			int cb = ParseHex(bb) * scaleHex;
			int ca = ParseHex(aa) * scaleHex;

			var rgba = Color.FromArgb(ca,cr,cg,cb);
			return rgba;
		}

		static int ParseHex(string h)
		{
			int num = int.Parse(h,NumberStyles.HexNumber);
			return num;
		}

		public static IColor? FromNameNative(string name)
		{
			var c = FromName(name);
			return c.HasValue
				? ImageHelpers.RgbaToNative(c.Value)
				: default(IColor?)
			;
		}
		public static Color? FromName(string name)
		{
			var c = Color.FromName(name);
			bool good = c.IsKnownColor && !c.IsSystemColor;
			if (!good) {
				good = TryMoreColorFromName(name,out c);
			}
			return good ? c : default(Color?);
		}

		public static IEnumerable<Color> AllColors()
		{
			return System.Linq.Enumerable.OrderBy(
				ColorList(),
				c => GetColorName(c)
			);
		}

		static IEnumerable<Color> ColorList()
		{
			foreach(KnownColor kc in Enum.GetValues(typeof(KnownColor))) {
				var c = Color.FromKnownColor(kc);
				bool good = c.IsKnownColor && !c.IsSystemColor;
				if (good) { yield return c; }
			}
			yield return ColorRebeccaPurple;
		}

		// use this instead of Color.Name
		public static string GetColorName(Color c)
		{
			string name = c.Name;
			if (StringComparer.OrdinalIgnoreCase.Equals(c,ColorRebeccaPurple)) { return NameRebeccaPurple; }
			if (!String.IsNullOrEmpty(name)) { return name; }
			return null;
		}

		static bool TryMoreColorFromName(string name, out Color color)
		{
			color = Color.Transparent;
			if (StringComparer.OrdinalIgnoreCase.Equals(name,NameRebeccaPurple)) {
				color = ColorRebeccaPurple;
				return true;
			}
			return false;
		}

		static Color ColorRebeccaPurple { get { return Color.FromArgb(0xFF,0x66,0x33,0x99); }}
		const string NameRebeccaPurple = "RebeccaPurple";
	}
}
