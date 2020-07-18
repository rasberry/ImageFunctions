using System;
using System.IO;
using System.Text;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions
{
	public abstract class AbstractFunction : IFunction
	{
		public void Main()
		{
			var Iic = Engines.Engine.GetConfig();
			using(var proc = CreateProcessor()) {
				proc.MaxDegreeOfParallelism = MaxDegreeOfParallelism;

				IImage img;
				if (InImage == null) {
					img = Iic.NewImage(Bounds.Width,Bounds.Height);
					Log.Debug($"Created image [{img.Width}x{img.Height}]");
				}
				else {
					img = Iic.LoadImage(InImage);
					Log.Debug($"Loaded image {InImage} [{img.Width}x{img.Height}]");
				}

				proc.Source = img;
				if (Bounds.IsEmpty) {
					Bounds = new Rectangle { Width = img.Width, Height = img.Height };
				}
				proc.Bounds = Bounds;
				proc.Apply();
				Iic.SaveImage(img,OutImage);
			}
		}

		public Rectangle Bounds { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }

		public abstract void Usage(StringBuilder sb);
		public abstract bool ParseArgs(string[] args);

		protected abstract AbstractProcessor CreateProcessor();
		protected string InImage = null;
		protected string OutImage = null;
	}
}
