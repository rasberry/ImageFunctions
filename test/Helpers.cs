using System;
using System.Collections.Generic;
using System.IO;
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

		public delegate string[] ArgsProvider();

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

		public static string[] Append(this string[] args,params string[] more)
		{
			var moreArgs = new List<string>(args);
			moreArgs.AddRange(more);
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

		public static (int,string[] args) ExtractInnards(object[] items)
		{
			int index = (int)items[0];
			string[] args = (string[])items[1];
			return (index,args);
		}

		public static string InFile(string n, bool forweb = false) {
			string file = n + ".png";
			return forweb ? "img/" + file : Path.Combine(Helpers.ImgRoot,file);
		}

		public static string CheckFile(Activity which, string n, int i,bool forweb = false) {
			string file = string.Format("img-{0}-{1}-{2}.png",(int)which,n,i+1);
			return forweb ? "img/" + file : Path.Combine(Helpers.ImgRoot,file);
		}
	}
}
