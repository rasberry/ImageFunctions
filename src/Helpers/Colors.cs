using System;

namespace ImageFunctions.Helpers
{
	public static class Colors
	{
		public static IFColor Black       { get { return RGB(0x00,0x00,0x00); }}
		public static IFColor IndianRed   { get { return RGB(0xCD,0x5C,0x5C); }}
		public static IFColor LimeGreen   { get { return RGB(0x32,0xCD,0x32); }}
		public static IFColor Magenta     { get { return RGB(0xFF,0x00,0xFF); }}
		public static IFColor Transparent { get { return RGB(0x00,0x00,0x00,0x00); }}
		public static IFColor White       { get { return RGB(0xFF,0xFF,0xFF); }}

		static IFColor RGB(int r,int g,int b,int a = 255)
		{
			return new IFColor(r/255.0,g/255.0,b/255.0,a/255.0);
		}
	}
}