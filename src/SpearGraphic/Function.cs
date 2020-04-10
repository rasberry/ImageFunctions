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
			var p = new Params(args);

			if (p.Default("-g",out O.Spear,Graphic.None).IsInvalid()) {
				return false;
			}
			if (p.Default("-bg",out O.BackgroundColor,Color.Transparent).IsInvalid()) {
				return false;
			}
			if (p.Default("-rs",out O.RandomSeed,null).IsInvalid()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,nameof(SpearGraphic)).IsInvalid()) {
				return false;
			}
			return true;
		}

		#if false
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
		#endif

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.SpearGraphic);
			sb.WL();
			sb.WL(0,name + " [options] [output image]");
			sb.WL(1,"Creates a spear graphic");
			sb.WL(1,"-g (name)"   ,"Choose which graphic to create");
			sb.WL(1,"-bg (color)" ,"Change Background color (default transparent)");
			sb.WL(1,"-rs (number)","Random Int32 seed value (defaults to system picked)");
			sb.WL();
			sb.WL(1,"Available Graphics");
			sb.PrintEnum<Graphic>(1);
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