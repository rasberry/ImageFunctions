using System;
using System.Drawing;
using System.Collections.Generic;
using ImageFunctions.Helpers;
using System.Collections.Concurrent;
using System.Text;
using System.Collections;

namespace ImageFunctions.ProbableImg
{
	public class Processor : AbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var Iis = Registry.GetImageEngine();
			var rect = Bounds;
			var frame = Source;

			using (var progress = new ProgressBar()) {
				CreateProfile(progress,frame,rect);

				//foreach(var kvp in Profile) {
				//	Log.Debug($"Key = {kvp.Key}");
				//	Log.Debug(kvp.Value.ToString());
				//}

				using (var canvas = Iis.NewImage(rect.Width,rect.Height))
				{
					CreateImage(progress,canvas);
					Source.BlitImage(canvas,rect);
				}
			}
			//	MoreHelpers.ThreadPixels(rect, MaxDegreeOfParallelism, (x,y) => {
			//		int cy = y - rect.Top;
			//		int cx = x - rect.Left;
			//		IColor nc = RunRule(Source,rect,x,y);
			//		canvas[cx,cy] = nc;
			//	},progress);
			//

		}

		public override void Dispose() {}

		void CreateProfile(ProgressBar pbar, IImage frame, Rectangle rect)
		{
			Profile = new ConcurrentDictionary<long,ColorProfile>();
			CToIndex = new Dictionary<IColor, long>();
			IToColor = new Dictionary<long, IColor>();

			pbar.Prefix = "Creating Profile ";
			MoreHelpers.ThreadPixels(rect, MaxDegreeOfParallelism, (x,y) => {
				int cy = y - rect.Top;
				int cx = x - rect.Left;
				var oc = frame[cx,cy];
				var maxx = frame.Width - 1;
				var maxy = frame.Height - 1;

				//Log.Debug($" BP [{x},{y}]");

				long ix = CtoI(oc);
				if (!Profile.ContainsKey(ix)) {
					var ncp = new ColorProfile {
						LockMe = new object(),
						NColor = new Dictionary<long, long>(),
						WColor = new Dictionary<long, long>(),
						SColor = new Dictionary<long, long>(),
						EColor = new Dictionary<long, long>()
					};
					Profile.TryAdd(ix,ncp);
				}

				IColor? cn = null, cw = null, cs = null, ce = null;
				if (cy > 0)    { cn = frame[cx, cy - 1]; }
				if (cx > 0)    { cw = frame[cx - 1, cy]; }
				if (cy < maxy) { cs = frame[cx, cy + 1]; }
				if (cx < maxx) { ce = frame[cx + 1, cy]; }

				var cc = Profile[ix];
				lock(cc.LockMe) {
					if (cn != null) { AddUpdateCount(cc.NColor,cn.Value); }
					if (cw != null) { AddUpdateCount(cc.WColor,cw.Value); }
					if (cs != null) { AddUpdateCount(cc.SColor,cs.Value); }
					if (ce != null) { AddUpdateCount(cc.EColor,ce.Value); }
				}
			},pbar);
		}

		void AddUpdateCount(IDictionary<long,long> dict, IColor color)
		{
			long ix = CtoI(color);
			if (dict.ContainsKey(ix)) {
				dict[ix]++;
			} else {
				dict.Add(ix,1);
			}
		}

		void CreateImage(ProgressBar pbar, IImage img)
		{
			pbar.Prefix = "Generating Image ";
			double totalPixels = img.Width * img.Height;
			double visitedPixels = 0;
			var pixStack = new List<(int,int)>();
			VisitNest = new BitArray[img.Height];
			for(int ii = 0; ii < img.Height; ii++) {
				VisitNest[ii] = new BitArray(img.Width,false);
			}

			Rnd = O.RandomSeed.HasValue
				? new Random(O.RandomSeed.Value)
				: new Random()
			;

			//pick a starting position
			int sx, sy;
			if (O.StartX != null && O.StartY != null) {
				sx = O.StartX.Value;
				sy = O.StartY.Value;
			}
			else {
				sx = Rnd.Next(img.Width);
				sy = Rnd.Next(img.Height);
			}

			pixStack.Add((sx,sy));
			long ix = PickStartColor();
			img[sx,sy] = ItoC(ix);
			DoVisit(sx,sy);
			int maxy = img.Height - 1;
			int maxx = img.Width - 1;

			while(pixStack.Count > 0) {
				//Log.Debug($"stack count={pixStack.Count}");

				//doing grab and swap so we can use list as a 'random pop' stack
				int rix = Rnd.Next(pixStack.Count);
				var (x,y) = pixStack[rix];
				pixStack[rix] = pixStack[pixStack.Count - 1];
				pixStack.RemoveAt(pixStack.Count - 1);

				pbar.Report(++visitedPixels / totalPixels);
				long cx = CtoI(img[x,y]);
				var profile = Profile[cx];

				if (y > 0)    { PickAndVisit(img, pixStack, profile.NColor, x, y - 1); }
				if (x > 0)    { PickAndVisit(img, pixStack, profile.WColor, x - 1, y); }
				if (y < maxy) { PickAndVisit(img, pixStack, profile.SColor, x, y + 1); }
				if (x < maxx) { PickAndVisit(img, pixStack, profile.EColor, x + 1, y); }
			}
		}

		bool IsVisited(int x, int y)
		{
			var ba = VisitNest[y];
			return ba[x];
		}

		void DoVisit(int x, int y)
		{
			var ba = VisitNest[y];
			ba[x] = true;
		}

		long PickStartColor()
		{
			long ix = Rnd.RandomLong(max: LastIndex);
			return ix;
		}

		void PickAndVisit(IImage img, List<(int,int)> stack, Dictionary<long,long> dict, int x, int y)
		{

			if (IsVisited(x,y)) {
				//Log.Debug($"already visited {x},{y}");
				return;
			}

			//find total number of buckets
			long total = 0;
			foreach(var kvp in dict) {
				total += kvp.Value;
			}

			long next = Rnd.RandomLong(total);
			long nc = 0;
			//this assumes the same order is kept between dictionary loops
			foreach(var kvp in dict) {
				total -= kvp.Value;
				if (total < next) {
					nc = kvp.Key;
				}
			}

			img[x,y] = ItoC(nc);
			DoVisit(x,y);
			stack.Add((x,y));
		}

		ConcurrentDictionary<long,ColorProfile> Profile;
		//use an index to save on some memory (TODO how much does this actually save?)
		Dictionary<IColor,long> CToIndex;
		Dictionary<long,IColor> IToColor;
		long LastIndex = 0;
		Random Rnd;
		BitArray[] VisitNest;

		public long CtoI(IColor c)
		{
			if (!CToIndex.TryGetValue(c,out long i)) {
				CToIndex[c] = LastIndex;
				IToColor[LastIndex] = c;
				LastIndex++;
			}
			return CToIndex[c];
		}

		public IColor ItoC(long i)
		{
			return IToColor[i];
		}

		class ColorProfile
		{
			public Dictionary<long,long> NColor;
			public Dictionary<long,long> WColor;
			public Dictionary<long,long> SColor;
			public Dictionary<long,long> EColor;
			public object LockMe;

			public override string ToString()
			{
				var sb = new StringBuilder();
				sb.AppendLine("\tNorth =====");
				DColorToString(sb,NColor);
				sb.AppendLine("\tWest  =====");
				DColorToString(sb,WColor);
				sb.AppendLine("\tSouth =====");
				DColorToString(sb,SColor);
				sb.AppendLine("\tEast  =====");
				DColorToString(sb,EColor);
				return sb.ToString();
			}

			void DColorToString(StringBuilder sb, Dictionary<long,long> d)
			{
				foreach(var kvp in d) {
					sb.AppendLine($"\t{kvp.Key} #={kvp.Value}");
				}
			}
		}
	}
}