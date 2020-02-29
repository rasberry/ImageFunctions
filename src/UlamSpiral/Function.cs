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
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-f") {
					O.UseFactorCount = true;
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
				else if (curr == "-cp" && ++a < len) {
					if (!OptionsHelpers.TryParseColor(args[a],out Color cp)) {
						Log.Error($"invalid color '{args[a]}'");
						return false;
					}
					O.ColorPrime = cp;
				}
				else if (curr == "-cf" && ++a < len) {
					if (!OptionsHelpers.TryParseColor(args[a],out Color cf)) {
						Log.Error($"invalid color '{args[a]}'");
						return false;
					}
					O.ColorComposite = cf;
				}
				else if (curr == "-bg" && ++a < len) {
					if (!OptionsHelpers.TryParseColor(args[a],out Color bg)) {
						Log.Error($"invalid color '{args[a]}'");
						return false;
					}
					O.ColorBack = bg;
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
					if (!OptionsHelpers.ParseNumberPercent(args[a],out double num)) {
						Log.Error($"Invalid number/percent {args[a]}");
						return false;
					}
					if (num < double.Epsilon) {
						Log.Error("dot size must be greater than zero");
						return false;
					}
					O.DotSize = num;
				}
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(nameof(UlamSpiral));
			}
			if (O.Mapping == PickMapping.None) {
				O.Mapping = PickMapping.Spiral;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.UlamSpiral);
			sb.AppendLine();
			sb.AppendLine(name + " [options] [output image]");
			sb.AppendLine(" Creates an Ulam spiral graphic ");
			sb.AppendLine(" -f                          Color pixel based on number of divisors");
			sb.AppendLine(" -c (x,y)                    Center x,y coordinate (default 0,0)");
			sb.AppendLine(" -m (mapping)                Mapping used to translate x,y into an index number (default spiral)");
			sb.AppendLine(" -s (number)                 Spacing between points (default 1)");
			sb.AppendLine(" -ds (number)[%]             Maximum dot size (absolute or relative) (default 100%)");
			sb.AppendLine(" -cp (color)                 Color of primes (default white)");
			sb.AppendLine(" -cf (color)                 Color of composites (default white)");
			sb.AppendLine(" -bg (color)                 Background color (default black)");
			sb.AppendLine();
			sb.AppendLine(" Available Mappings:");
			OptionsHelpers.PrintEnum<PickMapping>(sb,true,MappingDesc,null);
		}

		static string MappingDesc(PickMapping m)
		{
			switch(m)
			{
			case PickMapping.Diagonal: return "Diagonal winding from top left";
			case PickMapping.Linear:   return "Linear mapping left to right, top to bottom";
			case PickMapping.Spiral:   return "Spiral mapping inside to outside";
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