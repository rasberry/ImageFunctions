using System;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public enum Action
	{
		None = 0,
		PixelateDetails = 1,
		Derivatives = 2,
		AreaSmoother = 3,
		AreaSmoother2 = 4,
		ZoomBlur = 5,
		Swirl = 6
	}

	public interface IFunction
	{
		void Usage(StringBuilder sb);
		bool ParseArgs(string[] args);
		Rectangle Rect { get; set; }
		int? MaxDegreeOfParallelism { get; set; }
		void Main();
	}
}