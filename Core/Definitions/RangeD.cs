namespace ImageFunctions.Core;

public readonly record struct RangeD
{
	public RangeD(double s, double e)
	{
		Start = s; End = e;
	}
	public double Start { get; }
	public double End { get; }

	public static readonly RangeD Empty;
	public readonly bool IsEmpty => Start == 0.0 && End == 0.0;

	public override readonly string ToString() => $"{{S={Start}, E={End}}}";
}
