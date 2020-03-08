using System;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace ImageFunctions.UlamSpiral
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get {
			return new Size(1024,1024);
		}}

		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override bool ParseArgs(string[] args)
		{
			//Log.Debug($"args=[{String.Join(",",args)}]");
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-f") {
					O.ColorComposites = true;
				}
				else if (curr == "-6m") {
					O.ColorPrimesBy6m = true;
				}
				else if (curr == "-p") {
					O.ColorPrimesForce = true;
				}
				else if (curr == "-c" && ++a < len) {
					if (!OptionsHelpers.TryParseRectangle(args[a],out var rect)) {
						Log.Error($"invalid point '{args[a]}'");
						return false;
					}
					O.CenterX = rect.Width;  //treat width as x
					O.CenterY = rect.Height; //treat height as y
				}
				else if (curr == "-m" && ++a < len) {
					PickMapping m;
					if (!OptionsHelpers.TryParse(args[a], out m)) {
						Log.Error($"invalid mapping '{args[a]}'");
						return false;
					}
					O.Mapping = m;
				}
				else if ((curr == "-c1" || curr == "-c2" || curr == "-c3" || curr == "-c4") && ++a < len) {
					if (!OptionsHelpers.TryParseColor(args[a],out Color color)) {
						Log.Error($"invalid color '{args[a]}'");
						return false;
					}
					switch(curr) {
						case "-c1": O.Color1 = color; break;
						case "-c2": O.Color2 = color; break;
						case "-c3": O.Color3 = color; break;
						case "-c4": O.Color4 = color; break;
					}
				}
				else if (curr == "-s" && ++a < len) {
					if (!OptionsHelpers.TryParse(args[a],out int space)) {
						Log.Error($"invalid number {args[a]}");
						return false;
					}
					if (space < 1) {
						Log.Error("spacing must be at least one");
						return false;
					}
					O.Spacing = space;
				}
				else if (curr == "-ds" && ++a < len) {
					if (!OptionsHelpers.TryParse(args[a],out double num)) {
						Log.Error($"Invalid number {args[a]}");
						return false;
					}
					if (num < double.Epsilon) {
						Log.Error("dot size must be greater than zero");
						return false;
					}
					O.DotSize = num;
				}
				else if (curr == "-dt" && ++a < len) {
					if (!OptionsHelpers.TryParse(args[a],out PickDot dot)) {
						Log.Error($"invalid dot type {args[a]}");
						return false;
					}
					O.WhichDot = dot;
				}
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			//option defaults
			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(nameof(UlamSpiral));
			}
			if (O.Mapping == PickMapping.None) {
				O.Mapping = PickMapping.Spiral;
			}
			if (O.WhichDot == PickDot.None) {
				O.WhichDot = PickDot.Circle;
			}
			if (O.ColorPrimesBy6m && O.ColorPrimesForce) {
				O.ColorPrimesForce = false; //this is redundant when using -6m so turn it off
			}
			if (O.ColorPrimesBy6m) {
				if (!O.Color2.HasValue) { O.Color2 = Color.LimeGreen; }
				if (!O.Color4.HasValue) { O.Color4 = Color.IndianRed; }
			}
			if (O.ColorComposites) {
				if (!O.Color3.HasValue) { O.Color3 = Color.White; }
			}
			if (!O.Color1.HasValue) { O.Color1 = Color.Black; }
			if (!O.Color2.HasValue) { O.Color2 = Color.White; }

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.UlamSpiral);
			sb.AppendLine();
			sb.AppendLine(name + " [options] [output image]");
			sb.AppendLine(" Creates an Ulam spiral graphic ");
			sb.AppendLine(" -p                          Color pixel if prime (true if -f not specified)");
			sb.AppendLine(" -f                          Color pixel based on number of divisors; dot size is proportional to divisor count");
			sb.AppendLine(" -6m                         Color primes depending on if they are 6*m+1 or 6*m-1");
			sb.AppendLine(" -c (x,y)                    Center x,y coordinate (default 0,0)");
			sb.AppendLine(" -m (mapping)                Mapping used to translate x,y into an index number (default spiral)");
			sb.AppendLine(" -s (number)                 Spacing between points (default 1)");
			sb.AppendLine(" -ds (number)                Maximum dot size in pixels; decimals allowed (default 1.0)");
			sb.AppendLine(" -dt (dot type)              Dot used for drawing (default circle)");
			sb.AppendLine(" -c(1,2,3,4) (color)         Colors to be used depending on mode. (setting any of the colors is optional)");
			sb.AppendLine();
			sb.AppendLine(" Color Mappings");
			sb.AppendLine(" default: c1=background  c2=primes");
			sb.AppendLine(" -f     : c1=background  c2=primes  c3=composites");
			sb.AppendLine(" -6m    : c1=background  c2=6m-1    c3=composites  c4=6m+1");
			sb.AppendLine();
			sb.AppendLine(" Available Mappings:");
			OptionsHelpers.PrintEnum<PickMapping>(sb,true,DescMapping,null);
			sb.AppendLine();
			sb.AppendLine(" Available Dot Types:");
			OptionsHelpers.PrintEnum<PickDot>(sb,true,DescDotType,null);
		}

		static string DescMapping(PickMapping m)
		{
			switch(m)
			{
			case PickMapping.Diagonal: return "Diagonal winding from top left";
			case PickMapping.Linear:   return "Linear mapping left to right, top to bottom";
			case PickMapping.Spiral:   return "Spiral mapping inside to outside";
			}
			return "";
		}

		static string DescDotType(PickDot m)
		{
			switch(m)
			{
			case PickDot.Blob:   return "Draws a spherical fading dot";
			case PickDot.Circle: return "Draws a regular circle";
			case PickDot.Square: return "Draws a regular square";
			}
			return "";
		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
	}
}