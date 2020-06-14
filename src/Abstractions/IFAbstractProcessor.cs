using System;
using System.Drawing;

namespace ImageFunctions
{
	public abstract class IFAbstractProcessor : IDisposable
	{
		public abstract void Apply();
		public abstract void Dispose();

		public IFImage Source { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }
		public Rectangle Bounds { get; set; }
	}
}