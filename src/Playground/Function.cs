
#if DEBUG
using System;
using System.Drawing;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Playground
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get { return new Size(1024,1024); }}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.DefaultFile(out OutImage,nameof(Playground)).IsBad()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.Playground);
			sb.WL();
			sb.WL(0,name + " [options] [output image]");
			sb.WL(1,"does some kind of test ");
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor();
		}
	}
}
#endif