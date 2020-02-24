using System;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageFunctions
{
	/// <summary>
	/// Color Type that uses IEEE 64-bit float to store color values
	/// </summary>
	public struct RgbaD : IEquatable<RgbaD>, IPixel<RgbaD>
	{
		public RgbaD(double r,double g,double b,double a)
		{
			//Log.Debug($"RgbaD [{r},{g},{b},{a}]");
			//bool isValid =
			//	   r >= 0.0 && g >= 0.0 && b >= 0.0 && a >= 0.0
			//	&& r <= 1.0 && g <= 1.0 && b <= 1.0 && a <= 1.0;
			//if (!isValid) {
			//	throw new ArgumentOutOfRangeException("Valid range for components is [0.0,1.0]");
			//}
			R = Math.Clamp(r,0.0,1.0);
			G = Math.Clamp(g,0.0,1.0);
			B = Math.Clamp(b,0.0,1.0);
			A = Math.Clamp(a,0.0,1.0);
		}

		public double R;
		public double G;
		public double B;
		public double A;

		public static bool operator == (RgbaD lhs, RgbaD rhs) {
			return
				   lhs.R == rhs.R
				&& lhs.G == rhs.G
				&& lhs.B == rhs.B
				&& lhs.A == rhs.A
			;
		}

		public static bool operator != (RgbaD lhs, RgbaD rhs) {
			return !(lhs == rhs);
		}

		public bool Equals(RgbaD compare)
		{
			return this == compare;
		}

		public override bool Equals(object compare)
		{
			var right = (RgbaD)compare;
			return this == right;
		}

		public override int GetHashCode()
		{
			int hc = R.GetHashCode();
			hc = CombineHashCodes(hc,G.GetHashCode());
			hc = CombineHashCodes(hc,B.GetHashCode());
			hc = CombineHashCodes(hc,A.GetHashCode());
			return hc;
		}

		//https://github.com/microsoft/referencesource/blob/3b1eaf5203992df69de44c783a3eda37d3d4cd10/System.Numerics/System/Numerics/HashCodeHelper.cs
		static int CombineHashCodes(int h1, int h2)
		{
			return (((h1 << 5) + h1) ^ h2);
		}

		public PixelOperations<RgbaD> CreatePixelOperations()
		{
			return new PixelOperations<RgbaD>();
		}

		public void FromScaledVector4(Vector4 v)
		{
			this.R = v.X;
			this.G = v.Y;
			this.B = v.Z;
			this.A = v.W;
		}

		public Vector4 ToScaledVector4()
		{
			return new Vector4((float)R,(float)G,(float)B,(float)A);
		}

		public void FromVector4(Vector4 v)
		{
			FromScaledVector4(v);
		}

		public Vector4 ToVector4()
		{
			return ToScaledVector4();
		}

		public void FromArgb32(Argb32 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromBgra5551(Bgra5551 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromBgr24(Bgr24 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromBgra32(Bgra32 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromGray8(Gray8 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromGray16(Gray16 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromRgb24(Rgb24 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromRgba32(Rgba32 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void ToRgba32(ref Rgba32 dest)
		{
			dest.FromScaledVector4(ToScaledVector4());
		}

		public void FromRgb48(Rgb48 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromRgba64(Rgba64 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}
	}
}