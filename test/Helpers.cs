using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ImageFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ImageFunctions.Helpers;


namespace test
{
	public static class Helpers
	{
		public static void Debug(string message)
		{
			if (sw == null) {
				string file = Path.Combine(ProjectRoot,"test-log.txt");
				var fs = File.Open(file,FileMode.Create,FileAccess.Write,FileShare.Read);
				sw = new StreamWriter(fs);
				sw.WriteLine("@Created at "+DateTime.Now.ToString("O"));
			}
			sw.WriteLine(message);
			sw.Flush();
		}
		static StreamWriter sw = null;

		public static string ProjectRoot { get
		{
			if (RootFolder == null) {
				string root = AppContext.BaseDirectory;
				int i=40;
				while(--i > 0) {
					string f = new DirectoryInfo(root).Name;
					if (string.Equals(f,nameof(ImageFunctions),StringComparison.CurrentCultureIgnoreCase)) {
						break;
					} else {
						root = new Uri(Path.Combine(root,"..")).LocalPath;
					}
				}
				RootFolder = root;
			}
			return RootFolder;
		}}
		static string RootFolder = null;

		public static string WikiRoot { get {
			return Path.Combine(Helpers.ProjectRoot,"wiki");
		}}

		public static string ImgRoot { get {
			return Path.Combine(WikiRoot,"img");
		}}

		public static void RunTestWithInputFiles(Activity act, int index, IAmTestSomeOne test,
			Func<string,string,bool> fileComparer = null)
		{
			var images = test.GetImageNames();
			var argsForIndex = test.GetArgs(index);

			using(var tempFile = Helpers.CreateTempPngFile())
			{
				var imgs = images[0];
				var inFiles = Helpers.InFile(imgs);
				string outFile = tempFile.TempFileName;
				string checkFile = Helpers.CheckFile(act,imgs,index);
				var args = Helpers.Append(argsForIndex,inFiles,outFile);

				Helpers.RunImageFunction(act,args,outFile,checkFile,null,fileComparer);
			}
		}

		public static void RunTestGenerator(Activity act, int index, IAmTestNoneOne test, Func<string,string,bool> fileComparer = null)
		{
			var argsForIndex = test.GetArgs(index);
			string name = test.GetOutName(index);
			var bounds = test.GetBounds(index);

			using(var tempFile = Helpers.CreateTempPngFile())
			{
				string outFile = tempFile.TempFileName;
				string checkFile = Helpers.CheckFile(act,name,index);
				var args = Helpers.Append(argsForIndex,outFile);

				Helpers.RunImageFunction(act,args,outFile,checkFile,bounds,fileComparer);
			}
		}

		public static void RunImageFunction(Activity act, string[] args, string outFile, string checkFile,
			Rectangle? bounds = null,Func<string,string,bool> fileComparer = null)
		{
			Log.Debug($"RunImageFunction act={act} args=[{String.Join(",",args)}] outFile={outFile} checkFile={checkFile}");
			if (fileComparer == null) {
				fileComparer = Helpers.AreImagesEqual;
			}
			IFunction func = Registry.Map(act);
			if (bounds != null) { func.Bounds = bounds.Value; }
			bool worked = func.ParseArgs(args);
			Assert.IsTrue(worked,$"Unable to parse args. [{String.Join(",",args)}]");

			if (System.Diagnostics.Debugger.IsAttached) {
				func.MaxDegreeOfParallelism = 1;
			}
			func.Main();

			Assert.IsTrue(File.Exists(outFile),$"File does not exist {outFile}");
			Assert.IsTrue(File.Exists(checkFile),$"File does not exist {checkFile}");
			Assert.IsTrue(fileComparer(checkFile,outFile),$"Files do not match [{checkFile},{outFile}");
		}

		public static bool AreImagesEqual(string one, string two)
		{
			var iis = ImageFunctions.Engines.Engine.GetConfig();
			// Log.Debug($"AreImagesEqual one={one} two={two}");
			var iOne = iis.LoadImage(one);
			var iTwo = iis.LoadImage(two);
			using (iOne) using(iTwo) {
				if (iOne.Width != iTwo.Width || iOne.Height != iTwo.Height) {
					return false;
				}
				if (!AreFramesEqual(iOne,iTwo)) {
					return false;
				}
			}
			return true;
		}

		public static bool AreFramesEqual(IImage one, IImage two)
		{
			if (one.Width != two.Width || one.Height != two.Height) {
				return false;
			}

			for(int y = 0; y < one.Height; y++) {
				for(int x = 0; x < one.Width; x++) {
					var po = one[x,y];
					var pt = two[x,y];
					bool same = AreColorsEqual(po,pt);
					if (!same) { return false; }
				}
			}
			return true;
		}

		public static bool AreColorsEqual(IColor one, IColor two)
		{
			bool same =
				one.A == two.A &&
				one.R == two.R &&
				one.G == two.G &&
				one.B == two.B
			;
			return same;
		}

		public static bool AreColorsEqual(Color one, Color two)
		{
			bool same =
				one.A == two.A &&
				one.R == two.R &&
				one.G == two.G &&
				one.B == two.B
			;
			return same;
		}

		public static void AssertAreEqual(Color exp, Color act)
		{
			//normalize the colors - comparing the same color created
			// from a name vs with FromArgb won't trigger a match
			Color c1 = Color.FromArgb(exp.A,exp.R,exp.G,exp.B);
			Color c2 = Color.FromArgb(act.A,act.R,act.G,act.B);
			Assert.AreEqual(c1,c2);
		}

		public static void AssertAreSimilar(IColor e, IColor a, double maxdiff = 0.0)
		{
			var dist = MetricHelpers.ColorDistance(e,a);
			Assert.IsTrue(dist > 0.0,$"Difference was zero. Expected > 0.0");
			Assert.IsTrue(dist < maxdiff,
				$"Color Distance {dist} > {maxdiff}."
				+ $" Expected <{e.R},{e.G},{e.B},{e.A}>"
				+ $" Actual <{a.R},{a.G},{a.B},{a.A}>"
			);
		}

		public static double ImageDistance(string one, string two)
		{
			var iis = ImageFunctions.Engines.Engine.GetConfig();
			var iOne = iis.LoadImage(one);
			var iTwo = iis.LoadImage(two);
			using (iOne) using(iTwo) {
				var total = FrameDistance(iOne,iTwo);
				return total;
			}
		}

		public static double FrameDistance(IImage one, IImage two)
		{
			var black = ColorHelpers.Black;
			int mw = Math.Max(one.Width,two.Width);
			int mh = Math.Max(one.Height,two.Height);

			double total = 0.0;
			for(int y = 0; y < mh; y++) {
				for(int x = 0; x < mw; x++) {
					var pOne = one.Width < x && one.Width < y ? one[x,y] : black;
					var pTwo = two.Width < x && two.Width < y ? two[x,y] : black;
					double dist = MetricHelpers.ColorDistance(pOne,pTwo);
					total += dist;
				}
			}
			return total;
		}

		public static string[] Append(this string[] args,params object[] more)
		{
			var moreArgs = new List<string>(args);
			foreach(object o in more)
			{
				if (o is ITuple) {
					var tuple = o as ITuple;
					moreArgs.AddRange(tuple.Enumerate<string>());
				}
				else {
					moreArgs.Add(o.ToString());
				}
			}
			return moreArgs.ToArray();
		}

		public static ITempFile CreateTempPngFile()
		{
			return new TempPngFile();
		}

		public static ITuple InFile(ITuple tuple, bool forweb = false) {
			var pathTuple = tuple.Enumerate<string>()
				.Select((name) => {
					string file = name + ".png";
					string path = forweb ? "img/" + file : Path.Combine(Helpers.ImgRoot,file);
					return path;
				})
				.ToTuple();
			return pathTuple;
		}

		public static string CheckFile(Activity which, string name, int i, bool forweb = false) {
			string file = string.Format("img-{0}-{1}-{2}.png",(int)which,name,i+1);
			return forweb ? "img/" + file : Path.Combine(Helpers.ImgRoot,file);
		}

		public static string CheckFile(Activity which, ITuple tuple, int i,bool forweb = false) {
			string name = string.Join('-',tuple.Enumerate<string>());
			return CheckFile(which,name,i,forweb);
		}

		public static ITuple[] Tupleify<T>(this T[] array)
		{
			var dest = new ITuple[array.Length];
			for(int i=0; i<array.Length; i++) {
				dest[i] = Tuple.Create(array[i]);
			}
			return dest;
		}

		public static IEnumerable<T> Enumerate<T>(this ITuple tuple)
		{
			//TODO maybe handle nested tuples ?
			for(int t=0; t<tuple.Length; t++) {
				yield return (T)tuple[t];
			}
		}

		public static ITuple ToTuple<T>(this IEnumerable<T> items)
		{
			//need to unroll in reverse so put the whole thing in a list
			var bag = System.Linq.Enumerable.ToList(items);

			int residue = bag.Count % 7;
			ITuple last = null;
			int end = bag.Count - 1;

			switch(residue)
			{
			case 6:
				last = Tuple.Create(bag[end-5],bag[end-4],bag[end-3],bag[end-2],bag[end-1],bag[end]);
				end -= 6; break;
			case 5:
				last = Tuple.Create(bag[end-4],bag[end-3],bag[end-2],bag[end-1],bag[end]);
				end -= 5; break;
			case 4:
				last = Tuple.Create(bag[end-3],bag[end-2],bag[end-1],bag[end]);
				end -= 4; break;
			case 3:
				last = Tuple.Create(bag[end-2],bag[end-1],bag[end]);
				end -= 3; break;
			case 2:
				last = Tuple.Create(bag[end-1],bag[end]);
				end -= 2; break;
			case 1:
				last = Tuple.Create(bag[end]);
				end -= 1; break;
			}

			while(end > 0) {
				ITuple next;
				if (last == null) {
					next = Tuple.Create(bag[end-6],bag[end-5],bag[end-4],bag[end-3],bag[end-2],bag[end-1],bag[end]);
				} else {
					next = Tuple.Create(bag[end-6],bag[end-5],bag[end-4],bag[end-3],bag[end-2],bag[end-1],bag[end],last);
				}
				end -= 7;
				last = next;
			}
			return last;
		}
	}

	[TestClass]
	public class TestSetup
	{
		[AssemblyInitialize]
		public static void Init(TestContext ctx)
		{
			var cp = System.Diagnostics.Process.GetCurrentProcess();
			cp.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
		}

	}

}
