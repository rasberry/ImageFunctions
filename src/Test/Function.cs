using System;
using System.Drawing;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Test
{
	public class Function : IFAbstractFunction, IFGenerator
	{
		public Size StartingSize { get { return new Size(1024,1024); }}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.DefaultFile(out OutImage,nameof(Test)).IsBad()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
		}

		protected override IFAbstractProcessor CreateProcessor()
		{
			return new Processor();
		}
	}
}