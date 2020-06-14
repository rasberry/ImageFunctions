using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions
{
	public abstract class IFAbstractFunction : IFFunction, IFunction
	{
		public void Main()
		{
			var Iic = Engines.Engine.GetConfig();
			using(var proc = CreateProcessor()) {
				proc.MaxDegreeOfParallelism = MaxDegreeOfParallelism;

				IFImage img;
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
					Bounds = new System.Drawing.Rectangle { Width = img.Width, Height = img.Height };
				}
				proc.Bounds = Bounds;
				proc.Apply();
				Iic.SaveImage(img,OutImage);
			}
		}

		public System.Drawing.Rectangle Bounds { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }
		SixLabors.Primitives.Rectangle IFunction.Bounds {
			get {
				return new SixLabors.Primitives.Rectangle {
					X = Bounds.X, Y = Bounds.Y,
					Width = Bounds.Width, Height = Bounds.Height
				};
			}
			set {
				Bounds = new System.Drawing.Rectangle {
					X = value.X, Y = value.Y,
					Width = value.Width, Height = value.Height
				};
			}
		}

		public abstract void Usage(StringBuilder sb);
		public abstract bool ParseArgs(string[] args);

		protected abstract IFAbstractProcessor CreateProcessor();
		protected string InImage = null;
		protected string OutImage = null;
	}
}
