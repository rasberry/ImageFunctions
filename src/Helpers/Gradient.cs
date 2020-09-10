using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageFunctions.Helpers
{
	public class GrayGradient : IGradient
	{
		public IColor GetColor(double index)
		{
			index = Math.Clamp(index,0.0,1.0);
			double pct = Math.Log(1.0 + index,2.0);
			return new IColor(pct,pct,pct,1.0);
		}
	}

	public class FullRangeRGBGradient : IGradient
	{
		public IColor GetColor(double index)
		{
			index = Math.Clamp(index,0.0,1.0);

			//iterate HSL L=[0 to 1] S=1 H[0 to 1]
			double l = index;
			double s = 1.0;
			double h = (index % 4.0) / 4.0; //4 slows down the cycle 4x

			return HSLColor.ToNative(new ColorSpaceHsl.HSL(h,s,l));
		}
		static ColorSpaceHsl HSLColor = new ColorSpaceHsl();
	}

	public class GimpGGRGradient : IGradient
	{
		// https://github.com/jjgreen/cptutils/blob/master/src/common/ggr.c
		// https://nedbatchelder.com/code/modules/ggr.py
		// https://stackoverflow.com/questions/3462295/exporting-gimp-gradient-file

		public GimpGGRGradient(string ggrfile)
		{
			using (var fs = File.Open(ggrfile,FileMode.Open,FileAccess.Read,FileShare.Read)) {
				Grad = LoadGradient(fs);
			}
		}

		public GimpGGRGradient(Stream ggrStream)
		{
			Grad = LoadGradient(ggrStream);
		}

		public IColor GetColor(double index)
		{
			index = Math.Clamp(index,0.0,1.0);
			return GradientColor(index,Grad);
		}

		Gradient Grad = null;

		static ColorSpaceHsv ColorHSV = new ColorSpaceHsv();
		static IColor GradientColor(double z, Gradient grad)
		{
			double r = 0, g = 0, b = 0, a = 0;
			if (grad == null) {
				return new IColor(r,g,b,a);
			}

			z = Math.Min(1.0,Math.Max(0.0,z));
			var seg = GetSegmentAt(grad,z);

			double middle,factor = 0;
			double seglen = seg.Right - seg.Left;
			if (seglen < double.Epsilon) {
				middle = 0.5;
				z = 0.5;
			} else {
				middle = (seg.Middle - seg.Left) / seglen;
				z = (z - seg.Left) / seglen;
			}

			switch(seg.Type)
			{
			case GradType.Linear:
				factor = CalcLinearFactor(middle,z);
				break;
			case GradType.Curved:
				if (middle < double.Epsilon) {
					middle = double.Epsilon;
				}
				factor = Math.Pow(z, Math.Log(0.5) / Math.Log(middle));
				break;
			case GradType.Sine:
				z = CalcLinearFactor(middle,z);
				factor = (Math.Sin((-Math.PI/2.0) + Math.PI*z) + 1.0)/2.0;
				break;
			case GradType.SphereInc:
				z = CalcLinearFactor(middle,z) - 1.0;
				factor = Math.Sqrt(1.0 - z*z);
				break;
			case GradType.SphereDec:
				z = CalcLinearFactor(middle,z);
				factor = 1.0 - Math.Sqrt(1.0 - z*z);
				break;
			default:
				throw new ArgumentException("Corrupt gradient");
			}

			a = seg.A0 + (seg.A1 - seg.A0) * factor;

			if (seg.Color == GradColor.RGB)
			{
				r = seg.R0 + (seg.R1 - seg.R0) * factor;
				g = seg.G0 + (seg.G1 - seg.G0) * factor;
				b = seg.B0 + (seg.B1 - seg.B0) * factor;
			}
			else
			{
				var hsv0 = ColorHSV.ToSpace(new IColor(seg.R0,seg.G0,seg.B0,1.0));
				var hsv1 = ColorHSV.ToSpace(new IColor(seg.R1,seg.G1,seg.B1,1.0));
				double h0 = hsv0.H, s0 = hsv0.S, v0 = hsv0.V;
				double h1 = hsv1.H, s1 = hsv1.S, v1 = hsv1.V;

				s0 = s0 + (s1 - s0) * factor;
				v0 = v0 + (v1 - v0) * factor;

				switch(seg.Color)
				{
				case GradColor.HSVccw:
					if (h0 < h1) {
						h0 = h0 + (h1 - h0) * factor;
					} else {
						h0 = h0 + (1.0 - (h0 - h1)) * factor;
						if (h0 > 1.0) {
							h0 -= 1.0;
						}
					}
					break;
				case GradColor.HSVcw:
					if (h1 < h0) {
						h0 = h0 - (h0 - h1) * factor;
					} else {
						h0 = h0 - (1.0 - (h1 - h0)) * factor;
						if (h0 < 0.0) {
							h0 += 1.0;
						}
					}
					break;
				default:
					throw new ArgumentException("unknown color model");
				}

				var color = ColorHSV.ToNative(new ColorSpaceHsv.HSV(h0,s0,v0));
				r = color.R; g = color.G; b = color.B;
			}

			return new IColor(r,g,b,a);
		}

		static double CalcLinearFactor(double middle, double z)
		{
			if (z <= middle) {
				return middle < double.Epsilon ? 0.0 : 0.5 * z / middle;
			} else {
				z -= middle;
				middle = 1.0 - middle;
				return middle < double.Epsilon ? 1.0 : 0.5 + 0.5 * z / middle;
			}
		}

		static GradSegment GetSegmentAt(Gradient grad, double z)
		{
			foreach(var seg in grad.Segments)
			{
				if (z >= seg.Left && z <= seg.Right) {
					return seg;
				}
			}
			throw new ArgumentOutOfRangeException("no matching segment for "+z);
		}

		static Gradient LoadGradient(Stream ggrStream)
		{
			using (var sr = new StreamReader(ggrStream))
			{
				string line = sr.ReadLine();
				if(!line.StartsWith("GIMP Gradient")) {
					throw new FileLoadException("file does not seem to be a GIMP gradient");
				}
				return LoadGrad(sr);
			}
		}

		static Gradient LoadGrad(StreamReader sr)
		{
			Gradient g = new Gradient();

			string line = sr.ReadLine();
			if (line.StartsWith("Name:")) {
				g.Name = line.Substring(5).Trim();
			} else {
				g.Name = "Unnamed";
			}

			line = sr.ReadLine();
			if (!int.TryParse(line,out int numsegments) || numsegments < 1 || numsegments > MaxSegments) {
				throw new ArgumentOutOfRangeException("invalid number of segments");
			}

			var segList = new List<GradSegment>();
			for(int i=0; i<numsegments; i++)
			{
				line = sr.ReadLine();
				var seg = ParseSeg(line);
				segList.Add(seg);
			}
			g.Segments = segList.ToArray();

			return g;
		}

		static GradSegment ParseSeg(string line)
		{
			int element = 0;
			StringBuilder chunk = new StringBuilder();
			double num = double.NaN;
			GradSegment seg = new GradSegment();
			line += " ";

			foreach(char c in line)
			{
				if (!char.IsWhiteSpace(c)) {
					chunk.Append(c);
					continue;
				}

				double.TryParse(chunk.ToString(),out num);
				if (double.IsInfinity(num) || double.IsNaN(num)) {
					throw new ArgumentOutOfRangeException("unexpected value found");
				}

				switch(element)
				{
				case 00: seg.Left = num; break;
				case 01: seg.Middle = num; break;
				case 02: seg.Right = num; break;
				case 03: seg.R0 = num; break;
				case 04: seg.G0 = num; break;
				case 05: seg.B0 = num; break;
				case 06: seg.A0 = num; break;
				case 07: seg.R1 = num; break;
				case 08: seg.G1 = num; break;
				case 09: seg.B1 = num; break;
				case 10: seg.A1 = num; break;
				case 11: seg.Type = (GradType)((int)num); break;
				case 12: seg.Color = (GradColor)((int)num); break;
				}
				chunk.Clear();
				element++;
			}

			if (seg.Color == GradColor.HSVshort || seg.Color == GradColor.HSVlong)
			{
				var hsv0 = ColorHSV.ToSpace(new IColor(seg.R0,seg.G0,seg.B0,1.0));
				var hsv1 = ColorHSV.ToSpace(new IColor(seg.R1,seg.G1,seg.B1,1.0));
				seg.Color = GradHSVType(seg.Color,hsv0.H,hsv1.H);
			}
			if (!Enum.IsDefined(typeof(GradColor),seg.Color)) {
				throw new ArgumentOutOfRangeException("unknown color model");
			}
			return seg;
		}

		static GradColor GradHSVType(GradColor color,double x, double y)
		{
			double min = Math.Min(x,y);
			double max = Math.Max(x,y);

			double midlen = max - min;
			double rndlen = min + (1.0 - max);

			GradColor shorter;
			if (rndlen < midlen) {
				shorter = (max == y ? GradColor.HSVcw : GradColor.HSVccw);
			} else {
				shorter = (max == y ? GradColor.HSVccw : GradColor.HSVcw);
			}
			if (color == GradColor.HSVlong) {
				return shorter == GradColor.HSVcw ? GradColor.HSVccw : GradColor.HSVcw;
			} else if (color == GradColor.HSVshort) {
				return shorter;
			} else {
				return color;
			}
		}

		const int MaxSegments = 4096;

		enum GradType
		{
			Linear = 0,
			Curved,
			Sine,
			SphereInc,
			SphereDec
		}

		enum GradColor
		{
			RGB = 0,   /* normal RGB */
			HSVccw,    /* counterclockwise hue */
			HSVcw,     /* clockwise hue */
			HSVshort,  /* shorter of cw & ccw hue */
			HSVlong    /* longer of cw & ccw hue */
		}

		class GradSegment
		{
			public double Left = 0.0, Middle = 0.5, Right = 1.0;
			public double R0 = 0.0, G0 = 0.0, B0 = 0.0, A0 = 0.0;
			public double R1 = 1.0, G1 = 1.0, B1 = 1.0, A1 = 1.0;
			public GradType Type = GradType.Linear;
			public GradColor Color = GradColor.RGB;
		}

		class Gradient
		{
			public string Name;
			public GradSegment[] Segments;
		}
	}
}