using System;
using System.IO;
using System.Text;
using System.Drawing;
using ImageFunctions.Helpers;
using System.Collections.Generic;

namespace ImageFunctions.SpearGraphic
{
	public class Function : IFAbstractFunction, IFGenerator
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
			if (p.Default("-bg",out O.BackgroundColor,ColorHelpers.Transparent).IsInvalid()) {
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

		protected override IFAbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}

}