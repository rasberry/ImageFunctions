using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions.Swirl
{
	public class Function : IFunction
	{
		public void Main()
		{
			using (var img = Image.Load(InImage))
			{
				img.Mutate(Process);
				Helpers.SaveAsPng(OutImage,img);
			}
		}

		public Rectangle Rect { get; set; }

		public bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-cx" && ++a < len) {
					if (!Helpers.ParseDelimitedValues<int>(args[a],out int[] nums)) {
						Log.Error("Unable to parse delimited values");
						return false;
					}
					if (nums.Length < 2) {
						Log.Error("Two values must be provided");
						return false;
					}
					CenterPx = new Point(nums[0],nums[1]);
				}
				else if (curr == "-cp" && ++a < len) {
					if (!Helpers.ParseDelimitedValues<string>(args[a],out string[] nums)) {
						Log.Error("Unable to parse delimited values");
						return false;
					}
					if (nums.Length < 2) {
						Log.Error("Two values must be provided");
						return false;
					}
					if (!Helpers.ParseNumberPercent(nums[0],out double ppx)) {
						Log.Error("Could not parse "+nums[0]);
						return false;
					}
					if (!Helpers.ParseNumberPercent(nums[1],out double ppy)) {
						Log.Error("Could not parse "+nums[1]);
						return false;
					}
					CenterPp = new PointF((float)ppx,(float)ppy);
				}
				else if (curr == "-rx" && ++a < len) {
					if (!int.TryParse(args[a],out int val)) {
						Log.Error("Could not parse "+args[a]+" as a number");
						return false;
					}
					RadiusPx = val;
				}
				else if (curr == "-rp" && ++a < len) {
					if (!Helpers.ParseNumberPercent(args[a],out double val)) {
						Log.Error("Could not parse "+args[a]);
						return false;
					}
					RadiusPp = val;
				}
				else if (curr == "-s" && ++a < len) {
					if (!Helpers.ParseNumberPercent(args[a],out double val)) {
						Log.Error("Could not parse "+args[a]);
						return false;
					}
					Rotations = val;
				}
				else if (curr == "-ccw") {
					CounterClockwise = true;
				}
				else if (InImage == null) {
					InImage = curr;
				}
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			if (CenterPx == null && CenterPp == null) {
				CenterPp = new PointF(0.5f,0.5f);
			}
			if (RadiusPx == null && RadiusPp == null) {
				RadiusPp = 0.9;
			}
			if (String.IsNullOrEmpty(InImage)) {
				Log.Error("input image must be provided");
				return false;
			}
			if (!File.Exists(InImage)) {
				Log.Error("cannot find input image \""+InImage+"\"");
				return false;
			}
			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = Helpers.CreateOutputFileName(InImage);
			}
			return true;
		}

		public void Usage(StringBuilder sb)
		{
			string name = Helpers.FunctionName(Action.Swirl);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" TODO ");
			sb.AppendLine(" -cx (number),(number)       Swirl center X and Y coordinate in pixels");
			sb.AppendLine(" -cp (number),(number)       Swirl center X and Y coordinate proportionaly (default 50%,50%)");
			sb.AppendLine(" -rx (number)                Swirl radius in pixels");
			sb.AppendLine(" -rp (number)[%]             Swirl radius proportional to smallest image dimension (default 90%)");
			sb.AppendLine(" -s  (number)[%]             Number of rotations (default 0.9)");
			sb.AppendLine(" -ccw                        Rotate Counter-clockwise. (default is clockwise)");
		}

		void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.CenterPx = CenterPx;
			proc.CenterPp = CenterPp;
			proc.RadiusPx = RadiusPx;
			proc.RadiusPp = RadiusPp;
			proc.Rotations = Rotations;
			proc.CounterClockwise = CounterClockwise;

			if (Rect.IsEmpty) {
				ctx.ApplyProcessor(proc);
			} else {
				ctx.ApplyProcessor(proc,Rect);
			}

		}

		string InImage = null;
		string OutImage = null;
		Point? CenterPx = null;
		PointF? CenterPp = null;
		int? RadiusPx = null;
		double? RadiusPp = null;
		double Rotations = 0.9;
		bool CounterClockwise = false;
	}
}
