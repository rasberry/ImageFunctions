using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System.Collections.Generic;

namespace ImageFunctions.SpearGraphic
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get {
			return new Size(1024,1024);
		}}

		public override bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-g" && ++a < len) {
					if (!OptionsHelpers.TryParse<Graphic>(args[a],out Graphic pat)) {
						Log.Error("invalid graphic "+args[a]);
						return false;
					}
					O.Spear = pat;
				}
				else if (curr == "-bg" && ++a<len) {
					string clr = args[a];
					if (!OptionsHelpers.TryParseColor(clr,out Color c)) {
						Log.Error("invalid color \""+clr+"\"");
						return false;
					}
					O.BackgroundColor = c;
				}
				else if (curr == "-rs" && ++a<len) {
					if (!OptionsHelpers.TryParse<int>(args[a],out int rnd)) {
						Log.Error($"invalid number {curr}");
						return false;
					}
					O.RandomSeed = rnd;
				}
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(nameof(SpearGraphic));
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.SpearGraphic);
			sb.AppendLine();
			sb.AppendLine(name + " [options] [output image]");
			sb.AppendLine(" Creates a spear graphic");
			sb.AppendLine(" -g (name)                   Choose which graphic to create");
			sb.AppendLine(" -bg (color)                 Change Background color (default transparent)");
			sb.AppendLine(" -rs (number)                Random Int32 seed value (defaults to system picked)");
			sb.AppendLine();
			sb.AppendLine(" Available Graphics");

			OptionsHelpers.PrintEnum<Graphic>(sb,true);
		}

		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override void Main()
		{
			Main<Rgba32>();
		}

		Options O = new Options();
	}
}