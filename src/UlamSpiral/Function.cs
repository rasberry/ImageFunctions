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
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(nameof(UlamSpiral));
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
			sb.AppendLine();
		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
	}
}