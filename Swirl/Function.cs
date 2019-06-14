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
				if (false) {
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
			string name = Helpers.FunctionName(Action.Swirl);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" TODO ");
			//sb.AppendLine(" -H                          Horizontal only");
			//sb.AppendLine(" -V                          Vertical only");
		}

		void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			if (Rect.IsEmpty) {
				ctx.ApplyProcessor(proc);
			} else {
				ctx.ApplyProcessor(proc,Rect);
			}

		}

		string InImage = null;
		string OutImage = null;
	}
}
