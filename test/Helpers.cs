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
		public static string ProjectRoot { get {
			if (RootFolder == null) {
				RootFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
			}
			return RootFolder;
		}}
		static string RootFolder = null;

		public static string ImgRoot { get {
			return Path.Combine(ProjectRoot,"..","wiki","img");
		}}

		public static bool AreImagesEqual(string one, string two)
		{
			var iOne = Image.Load(one);
			var iTwo = Image.Load(two);
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

		public static void RunImageFunction(Activity act, List<string> args, string inFile, string checkFile)
		{
			string pr = Helpers.ProjectRoot;
			string outFile = Path.GetTempFileName();
			File.Move(outFile,outFile+".png");
			outFile += ".png";

			IFunction func = Registry.Map(act);
			args.Add(inFile);
			args.Add(outFile);

			bool worked = func.ParseArgs(args.ToArray());
			Assert.IsTrue(worked);

			func.Main();

			Assert.IsTrue(File.Exists(outFile));
			Assert.IsTrue(Helpers.AreImagesEqual(checkFile,outFile));
			File.Delete(outFile);
		}
	}
}