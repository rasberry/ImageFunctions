using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ImageFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace test
{
	public static class Helpers
	{
		public static void Debug(string message)
		{
			if (sw == null) {
				var fs = File.Open("test-log.txt",FileMode.Create,FileAccess.Write,FileShare.Read);
				sw = new StreamWriter(fs);
			}
			sw.WriteLine(message);
			sw.Flush();
		}
		static StreamWriter sw = null;

		public static string ProjectRoot { get {
			if (RootFolder == null) {
				RootFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
			}
			return RootFolder;
		}}
		static string RootFolder = null;

		public static string WikiRoot { get {
			return Path.Combine(Helpers.ProjectRoot,"..","wiki");
		}}

		public static string ImgRoot { get {
			return Path.Combine(WikiRoot,"img");
		}}

		public static void RunImageFunction(Activity act, string[] args, string outFile, string checkFile)
		{
			IFunction func = Registry.Map(act);
			bool worked = func.ParseArgs(args);
			Assert.IsTrue(worked);

			func.Main();

			Assert.IsTrue(File.Exists(outFile));
			Assert.IsTrue(File.Exists(checkFile));
			Assert.IsTrue(Helpers.AreImagesEqual(checkFile,outFile));
		}

		public static bool AreImagesEqual(string one, string two)
		{
			var iOne = Image.Load<Rgba32>(one);
			var iTwo = Image.Load<Rgba32>(two);
			using (iOne) using(iTwo) {
				if (!iOne.Bounds().Equals(iTwo.Bounds())) {
					return false;
				}
				if (iOne.Frames.Count != iTwo.Frames.Count) {
					return false;
				}
				for(int f=0; f<iOne.Frames.Count; f++) {
					var fOne = iOne.Frames[f];
					var fTwo = iTwo.Frames[f];
					if (!AreFramesEqual(fOne,fTwo)) {
						return false;
					}
				}
			}

			return true;
		}

		public static bool AreFramesEqual<TPixel>(ImageFrame<TPixel> one, ImageFrame<TPixel> two)
			where TPixel : struct, IPixel<TPixel>
		{
			if (!one.Bounds().Equals(two.Bounds())) {
				return false;
			}

			var sOne = one.GetPixelSpan();
			var sTwo = two.GetPixelSpan();

			return sOne.SequenceEqual(sTwo);
		}

		public static string[] Append(this string[] args,params object[] more)
		{
			var moreArgs = new List<string>(args);
			foreach(object o in more)
			{
				if (o is ITuple) {
					var tuple = o as ITuple;
					for(int t=0; t<tuple.Length; t++) {
						moreArgs.Add(tuple[t].ToString());
					}
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

		public static IEnumerable<Activity> AllActivity()
		{
			foreach(Activity a in Enum.GetValues(typeof(Activity))) {
				if (a == Activity.None) { continue; }
				yield return a;
			}
		}

		public static ITuple InFile(ITuple tuple, bool forweb = false) {
			int len = tuple.Length;
			var list = new List<string>();

			for(int t=0; t<tuple.Length; t++) {
				string file = tuple[t] + ".png";
				string path = forweb ? "img/" + file : Path.Combine(Helpers.ImgRoot,file);
				list.Add(path);
			}
			return list.ToTuple();
		}

		public static string CheckFile(Activity which, ITuple tuple, int i,bool forweb = false) {
			string name = string.Join('-',tuple.Enumerate<string>());
			string file = string.Format("img-{0}-{1}-{2}.png",(int)which,name,i+1);
			return forweb ? "img/" + file : Path.Combine(Helpers.ImgRoot,file);
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
}
