using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AutoWhiteBalance;

[InternalRegisterFunction(nameof(AutoWhiteBalance))]
public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			CoreOptions = core,
			Layers = layers
		};
		return f;
	}

	public IOptions Options { get { return O; }}

	// based on https://docs.gimp.org/2.8/en/gimp-layer-white-balance.html
	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Register)) {
			return false;
		}
		if(Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		var source = Layers.First().Canvas;
		using var progress = new ProgressBar();
		var hist = CalcHistorgram(progress, source, O.BucketCount);
		var factors = CalcStretchFactors(hist, source.Width, source.Height, O.DiscardRatio);

		int maxThreads = CoreOptions.MaxDegreeOfParallelism.GetValueOrDefault(1);
		progress.Prefix = "Modifying Colors ";
		source.ThreadPixels((int x, int y) => {
			Core.ColorSpace.IColor3 orig = source[x, y];
			var c1 = Math.Clamp((orig.C1 - factors.C1Shift) * factors.C1Stretch, 0.0, 1.0);
			var c2 = Math.Clamp((orig.C2 - factors.C2Shift) * factors.C2Stretch, 0.0, 1.0);
			var c3 = Math.Clamp((orig.C3 - factors.C3Shift) * factors.C3Stretch, 0.0, 1.0);
			double a = O.StretchAlpha
				? Math.Clamp((orig.A - factors.AShift) * factors.AStretch, 0.0, 1.0)
				: orig.A;
			source[x, y] = new ColorRGBA(c1, c2, c3, a);
		}, maxThreads, progress);

		return true;
	}

	static Stretch3Data CalcStretchFactors(Histogram3Data buckets,
		int w, int h, double discardPct)
	{
		long count = w * h;
		long floor = (long)(count * discardPct);
		var data = new Stretch3Data();

		var (c1High, c1Low) = FindHighLow(buckets.C1, floor);
		var (c2High, c2Low) = FindHighLow(buckets.C2, floor);
		var (c3High, c3Low) = FindHighLow(buckets.C3, floor);
		var (aHigh, aLow) = FindHighLow(buckets.A, floor);

		(data.C1Shift, data.C1Stretch) = CalcShiftStretch(c1High, c1Low, buckets.C1.Length);
		(data.C2Shift, data.C2Stretch) = CalcShiftStretch(c2High, c2Low, buckets.C2.Length);
		(data.C3Shift, data.C3Stretch) = CalcShiftStretch(c3High, c3Low, buckets.C3.Length);
		(data.AShift, data.AStretch) = CalcShiftStretch(aHigh, aLow, buckets.A.Length);

		return data;
	}

	static (double, double) CalcShiftStretch(int high, int low, int count)
	{
		//if we only have a single bucket don't do anything
		if(high <= low) { return (0.0, 1.0); }

		var shift = (double)low / count;
		var stretch = count / (double)(high - low);
		return (shift, stretch);
	}

	static (int, int) FindHighLow(long[] band, long floor)
	{
		int high = 0;
		int low = 0;
		for(int b = 0; b < band.Length; b++) {
			if(band[b] < floor) { low = b; }
			//stop at the first bucket that goes above floor
			else { break; }
		}
		for(int e = band.Length - 1; e >= 0; e--) {
			if(band[e] < floor) { high = e; }
			//stop at the first bucket that goes above floor
			else { break; }
		}
		return (high, low);
	}

	static Histogram3Data CalcHistorgram(ProgressBar pb, ICanvas canvas, int bucketCount)
	{
		pb.Prefix = "Calculating Histogram ";
		var buckets = new Histogram3Data(bucketCount);
		int lastIndex = bucketCount - 1;

		for(int y = 0; y < canvas.Height; y++) {
			for(int x = 0; x < canvas.Width; x++) {
				//using IColor3 to eventually support colorspaces
				Core.ColorSpace.IColor3 px = canvas[x, y];

				//since both 0.0 and 1.0 are valid values, multiply by lastIndex instead of bucketCount
				// to ensure we don't get a bias for bucketCount bucket being dumped into lastIndex bucket
				var b1 = Math.Clamp((int)(px.C1 * lastIndex), 0, lastIndex);
				var b2 = Math.Clamp((int)(px.C2 * lastIndex), 0, lastIndex);
				var b3 = Math.Clamp((int)(px.C3 * lastIndex), 0, lastIndex);
				var bA = Math.Clamp((int)(px.A * lastIndex), 0, lastIndex);

				buckets.C1[b1]++;
				buckets.C2[b2]++;
				buckets.C3[b3]++;
				buckets.A[bA]++;
			}
			var done = (double)y / canvas.Height;
			pb.Report(done);
		}
		return buckets;
	}

	readonly Options O = new();
	IRegister Register;
	ICoreOptions CoreOptions;
	ILayers Layers;

	class Histogram3Data
	{
		public Histogram3Data(int count)
		{
			C1 = new long[count];
			C2 = new long[count];
			C3 = new long[count];
			A = new long[count];
		}

		public long[] C1;
		public long[] C2;
		public long[] C3;
		public long[] A;
	}

	class Stretch3Data
	{
		public double C1Shift;
		public double C1Stretch;
		public double C2Shift;
		public double C2Stretch;
		public double C3Shift;
		public double C3Stretch;
		public double AShift;
		public double AStretch;
	}
}
