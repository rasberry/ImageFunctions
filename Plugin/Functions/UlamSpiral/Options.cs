using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.UlamSpiral;

public sealed class Options : IOptions
{
	public bool ColorComposites;
	public bool ColorPrimesBy6m;
	public bool ColorPrimesForce;
	public int? CenterX;
	public int? CenterY;
	public int Spacing;
	public double DotSize;
	public PickDot WhichDot;
	public PickMapping Mapping;
	public ColorRGBA? Color1;
	public ColorRGBA? Color2;
	public ColorRGBA? Color3;
	public ColorRGBA? Color4;
	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1,"Creates an Ulam spiral graphic ");
		sb.ND(1,"-p"                  ,"Color pixel if prime (true if -f not specified)");
		sb.ND(1,"-f"                  ,"Color pixel based on number of divisors; dot size is proportional to divisor count");
		sb.ND(1,"-6m"                 ,"Color primes depending on if they are 6*m+1 or 6*m-1");
		sb.ND(1,"-c (x,y)"            ,"Center x,y coordinate (default 0,0)");
		sb.ND(1,"-m (mapping)"        ,"Mapping used to translate x,y into an index number (default spiral)");
		sb.ND(1,"-s (number)"         ,"Spacing between points (default 1)");
		sb.ND(1,"-ds (number)"        ,"Maximum dot size in pixels; decimals allowed (default 1.0)");
		sb.ND(1,"-dt (dot type)"      ,"Dot used for drawing (default circle)");
		sb.ND(1,"-c(1,2,3,4) (color)" ,"Colors to be used depending on mode. (setting any of the colors is optional)");
		sb.WT();
		sb.ND(1,"Color Mappings:");
		sb.ND(1,"default","c1=background  c2=primes");
		sb.ND(1,"-f"     ,"c1=background  c2=primes  c3=composites");
		sb.ND(1,"-6m"    ,"c1=background  c2=6m-1    c3=composites  c4=6m+1");
		sb.WT();
		sb.ND(1,"Available Mappings:");
		sb.PrintEnum<PickMapping>(1,DescMapping, excludeZero: true);
		sb.WT();
		sb.ND(1,"Available Dot Types:");
		sb.PrintEnum<PickDot>(1,DescDotType, excludeZero: true);
	}

	static string DescMapping(PickMapping m)
	{
		switch(m)
		{
		case PickMapping.Diagonal: return "Diagonal winding from top left";
		case PickMapping.Linear:   return "Linear mapping left to right, top to bottom";
		case PickMapping.Spiral:   return "Spiral mapping inside to outside";
		}
		return "";
	}

	static string DescDotType(PickDot m)
	{
		switch(m)
		{
		case PickDot.Blob:   return "Draws a spherical fading dot";
		case PickDot.Circle: return "Draws a regular circle";
		case PickDot.Square: return "Draws a regular square";
		}
		return "";
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var pointParser = new ParseParams.Parser<Point>(PlugTools.ParsePoint);
		var colorParser = new ParseParams.Parser<ColorRGBA>(PlugTools.ParseColor);

		if (p.Has("-f").IsGood()) {
			ColorComposites = true;
		}
		if (p.Has("-6m").IsGood()) {
			ColorPrimesBy6m = true;
		}
		if (p.Has("-p").IsGood()) {
			ColorPrimesForce = true;
		}

		if (p.Scan("-c", par: pointParser)
			.WhenGood(r => {
				CenterX = r.Value.X;
				CenterY = r.Value.Y;
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-m", PickMapping.Spiral)
			.WhenGoodOrMissing(r => { Mapping = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		var colorParse = new ParseParams.Parser<Color>(ExtraParsers.ParseColor);

		if (p.Scan("-c1", par: colorParser)
			.WhenGood(r => { Color1 = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan("-c2", par: colorParser)
			.WhenGood(r => { Color2 = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan("-c3", par: colorParser)
			.WhenGood(r => { Color3 = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan("-c4", par: colorParser)
			.WhenGood(r => { Color4 = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-s", 1)
			.WhenGoodOrMissing(r => { Spacing = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan("-ds", 1.0)
			.WhenGoodOrMissing(r => { DotSize = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-dt", PickDot.Circle)
			.WhenGoodOrMissing(r => { WhichDot = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return true;
		}

		//color defaults - seperated since these are slightly complicated
		if (ColorPrimesBy6m && ColorPrimesForce) {
			ColorPrimesForce = false; //this is redundant when using -6m so turn it off
		}
		if (ColorPrimesBy6m) {
			if (!Color2.HasValue) { Color2 = PlugColors.LimeGreen; }
			if (!Color4.HasValue) { Color4 = PlugColors.IndianRed; }
		}
		if (ColorComposites) {
			if (!Color3.HasValue) { Color3 = PlugColors.White; }
		}
		if (!Color1.HasValue) { Color1 = PlugColors.Black; }
		if (!Color2.HasValue) { Color2 = PlugColors.White; }

		return true;
	}
}

public enum PickMapping {
	None = 0,
	Linear = 1,
	Diagonal = 2,
	Spiral = 3
}

public enum PickColor {
	None = 0,
	Back = 1,
	Prime = 2,
	Comp = 3,
	Prime2 = 4
}

public enum PickDot {
	None = 0,
	Blob = 1,
	Circle = 2,
	Square = 3
}