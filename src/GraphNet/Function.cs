using System;
using System.IO;
using System.Text;
using System.Drawing;
using ImageFunctions.Helpers;
using System.Collections.Generic;

namespace ImageFunctions.GraphNet
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get {
			return new Size(1024,1024);
		}}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Default("-b",out O.States,2).IsInvalid()) {
				return false;
			}
			if (p.Default("-n",out O.NodeCount,Bounds.Width).IsInvalid()) {
				return false;
			}
			if (p.Default("-c",out O.Connectivity,3).IsInvalid()) {
				return false;
			}
			if (p.Default("-rs",out O.RandomSeed,null).IsInvalid()) {
				return false;
			}
			if (p.Default("-p",out O.PertubationRate,0.0,OptionsHelpers.ParseNumberPercent).IsInvalid()) {
				return false;
			}

			if (p.DefaultFile(out OutImage,nameof(GraphNet)).IsBad()) {
				return false;
			}

			if (O.NodeCount < 1 || O.NodeCount > this.Bounds.Width) {
				Tell.MustBeBetween("-n","1",this.Bounds.Width.ToString());
				return false;
			}
			Log.Debug($"nodes = {O.NodeCount}");

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.GraphNet);
			sb.WL();
			sb.WL(0,name + " [options] [output image]");
			sb.WL(1,"Creates a plot of a boolean-like network with a random starring state.");
			sb.WL(1,"-b (number)"    ,"Number of states (default 2)");
			sb.WL(1,"-n (number)"    ,"Number of nodes in the network (defaults to width of image)");
			sb.WL(1,"-c (number)"    ,"Connections per node (default 3)");
			sb.WL(1,"-p (number)"    ,"Chance of inserting a perturbation (default 0)");
			sb.WL(1,"-rs (number)"   ,"Random Int32 seed value (defaults to system picked)");
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}
}