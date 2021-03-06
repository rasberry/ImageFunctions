using System;
using System.Text;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.ColatzVis
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get {
			return new Size(256,256);
		}}

		public override bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (false) {
				}
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(nameof(ColatzVis));
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			return; //TODO disabled for now
			
			#if false
			string name = OptionsHelpers.FunctionName(Activity.ColatzVis);
			sb.AppendLine();
			sb.AppendLine(name + " [options] [output image]");
			sb.AppendLine(" TODO Does something ");
			//sb.AppendLine(" -g (name)                   Choose which graphic to create");
			//sb.AppendLine(" -bg (color)                 Change Background color (default transparent)");
			sb.AppendLine();
			#endif
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}
}