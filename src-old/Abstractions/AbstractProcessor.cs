using System;
using System.Drawing;

namespace ImageFunctions
{
	public abstract class AbstractProcessor : IDisposable
	{
		public abstract void Apply();
		public abstract void Dispose();

		public IImage Source { get; set; }
		public int? MaxDegreeOfParallelism { get; set; }
		public Rectangle Bounds { get; set; }
	}
}