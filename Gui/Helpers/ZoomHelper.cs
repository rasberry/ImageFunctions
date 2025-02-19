namespace ImageFunctions.Gui.Helpers;

public class ZoomHelper
{
	int CurrentIndex = NormalZoomIndex;

	public double Smaller()
	{
		CurrentIndex = Math.Clamp(CurrentIndex + 1, 0, Levels.Length - 1);
		return Levels[CurrentIndex];
	}

	public double Bigger()
	{
		CurrentIndex = Math.Clamp(CurrentIndex - 1, 0, Levels.Length - 1);
		return Levels[CurrentIndex];
	}

	public double Reset()
	{
		CurrentIndex = NormalZoomIndex;
		return Levels[CurrentIndex];
	}

	public double Zoom {
		get => Levels[CurrentIndex];
	}

	// static int FindNearest(double zoom)
	// {
	// 	if(zoom >= Levels[0]) { return 0; }
	// 	if(zoom <= Levels[^1]) { return Levels.Length - 1; }

	// 	for(int d = Levels.Length - 2; d >= 0; d--) {
	// 		if(zoom < Levels[d]) {
	// 			double diffup = Levels[d] - zoom; //we know zoom is less than
	// 			double diffdn = zoom - Levels[d - 1]; //we know zoom is greater or equal
	// 			return diffup < diffdn ? d : d - 1;
	// 		}
	// 	}

	// 	//this should never happen
	// 	throw new InvalidOperationException($"Failed to find nearest zoom for {zoom}");
	// }

#pragma warning disable format

	// //these were copied derived from Gimp
	// const int NormalZoomIndex = 16;
	// static readonly double[] Levels = new double[] {
	// 	256.0, 180.0, 128.0,  90.0,  64.0,  45.0,
	// 	32.0,   23.0,  16.0,  11.0,   8.0,   5.5,
	// 	 4.0,    3.0,   2.0,   1.5,
	// 	 1.0,
	// 	 1/1.5,  1/2.0,  1/3.0,  1/4.0,
	// 	 1/5.5,  1/8.0,  1/11.0, 1/16.0,  1/23.0,  1/32.0,
	// 	 1/45.0, 1/64.0, 1/90.0, 1/128.0, 1/180.0, 1/256.0
	// };

	//these were copied derived from Gimp
	const int NormalZoomIndex = 12;
	static readonly double[] Levels = new double[] {
		64.0,  45.0,   32.0,   23.0,  16.0,  11.0,
		 8.0,   5.5,    4.0,    3.0,   2.0,   1.5,
		 1.0,
		1/1.5,  1/2.0,   1/3.0,   1/4.0,  1/5.5,  1/8.0,
		1/11.0, 1/16.0,  1/23.0,  1/32.0, 1/45.0, 1/64.0,
	};

#pragma warning restore format

}
