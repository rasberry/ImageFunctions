using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageFunctions.AreaSmoother2
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

		public bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-H") {
					HOnly = true;
				}
				else if (curr == "-V") {
					VOnly = true;
				}
				else if (InImage == null) {
					InImage = curr;
				}
				else if (OutImage == null) {
					OutImage = curr;
				}
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
			string name = Helpers.FunctionName(Action.AreaSmoother2);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Blends adjacent areas of flat color together by blending horizontal and vertical gradients");
			sb.AppendLine(" -H                          Horizontal only");
			sb.AppendLine(" -V                          Vertical only");
		}

		void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.HOnly = HOnly;
			proc.VOnly = VOnly;
			ctx.ApplyProcessor(proc);
		}

		string InImage = null;
		string OutImage = null;
		bool HOnly = false;
		bool VOnly = false;
	}
}
