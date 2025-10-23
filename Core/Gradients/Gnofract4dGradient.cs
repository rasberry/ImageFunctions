using ImageFunctions.Core.Aides;

namespace ImageFunctions.Core.Gradients;

/// <summary>Parser for Gnofract4d gradient files (.map)</summary>
public class Gnofract4dGradient : IColorGradient
{
	public Gnofract4dGradient(string gf4dfile)
	{
		using var fs = File.Open(gf4dfile, FileMode.Open, FileAccess.Read, FileShare.Read);
		LoadGradient(fs);
	}

	public Gnofract4dGradient(Stream gf4dStream)
	{
		LoadGradient(gf4dStream);
	}

	/// <inheritdoc/>
	public ColorRGBA GetColor(double position)
	{
		position = Math.Clamp(position, 0.0, 1.0);
		if(Stops.Count == 1) { return Stops[0]; }

		int last = Stops.Count - 1;
		double pos = position * last;
		double ratio = pos % 1.0; //get fractional part
		int ipos = (int)pos;

		if(ipos >= last) { //deal with index == 1.0
			return Stops[last];
		}
		var color = ColorAide.BetweenColor(Stops[ipos], Stops[ipos + 1], ratio);
		return color;
	}

	readonly List<ColorRGBA> Stops = new();

	void LoadGradient(Stream stream)
	{
		//there's usually 256 lines but not setting a limit here
		using var sr = new StreamReader(stream);
		while(!sr.EndOfStream) {
			string line = sr.ReadLine();
			string[] components = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if(components.Length < 1) {
				throw Squeal.InvalidArgument("Line with no components found");
			}

			double[] values = new double[components.Length];
			for(int c = 0; c < components.Length; c++) {
				if(!double.TryParse(components[c], out double val)) {
					throw Squeal.InvalidArgument($"Component '{components[c]}' does not appear to be a number");
				}
				values[c] = val;
			}

			double r = 0.0, g = 0.0, b = 0.0, a = 255.0;
			switch(components.Length) {
			case 1: r = values[0]; break;
			case 2: r = values[0]; g = values[1]; break;
			case 3: r = values[0]; g = values[1]; b = values[2]; break;
			default: // 4 or more
			case 4: r = values[0]; g = values[1]; b = values[2]; a = values[3]; break;
			}
			var color = new ColorRGBA(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
			Stops.Add(color);
		}
	}
}
