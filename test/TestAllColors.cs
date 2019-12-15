using System;
using System.Runtime.CompilerServices;
using ImageFunctions.AllColors;
using ImageFunctions.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.Primitives;
using System.IO;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestAllColors
	{
		[DataTestMethod]
		[DynamicData(nameof(PatternsData), DynamicDataSourceType.Method)]
		public void GenAllPatterns(Pattern pt)
		{
			string outName = Path.Combine(Helpers.ProjectRoot,"test-"+pt+".png");
			if (File.Exists(outName)) { return; }
			var func = new ImageFunctions.AllColors.Function();
			func.ParseArgs(new string[] { "-p",pt.ToString(),outName });
			func.Bounds = new Rectangle(0,0,func.StartingSize.Width,func.StartingSize.Width);
			func.Main();
		}

		public static IEnumerable<object[]> PatternsData()
		{
			var allPatterns = OptionsHelpers.EnumAll<Pattern>();
			foreach(Pattern pt in allPatterns) {
				yield return new object[] { pt };
			}
		}
		[DataTestMethod]
		[DynamicData(nameof(SpacesData), DynamicDataSourceType.Method)]
		public void GenAllSpaces(Space sp, int[] order)
		{
			var func = new ImageFunctions.AllColors.Function();
			string sso = string.Join(',',order.Select(n => n.ToString()));
			string outName = Path.Combine(Helpers.ProjectRoot,"test-" + sp + "-" + sso.Replace(",", "") + ".png");
			if (File.Exists(outName)) { return; }
			func.ParseArgs(new string[] { "-s", sp.ToString(), "-so", sso, outName });
			func.Bounds = new Rectangle(0,0,func.StartingSize.Width,func.StartingSize.Width);
			func.Main();
		}

		public static IEnumerable<object[]> SpacesData()
		{
			var allSpaces = OptionsHelpers.EnumAll<Space>();
			foreach(Space sp in allSpaces) {
				int[] order = null;
				int count = 0;
				while(true) {
					order = sp == Space.Cmyk
						? GetCombo4(count)
						: GetCombo3(count);
					if (order == null) { break; }
					count++;
					yield return new object[] { sp, order };
				}
			}
		}

		static int[] GetCombo3(int which)
		{
			switch(which)
			{
			case 0: return new int[] { 1,2,3 };
			case 1: return new int[] { 1,3,2 };
			case 2: return new int[] { 2,3,1 };
			case 3: return new int[] { 2,1,3 };
			case 4: return new int[] { 3,1,2 };
			case 5: return new int[] { 3,2,1 };
			}
			return null;
		}
		static int[] GetCombo4(int which)
		{
			switch(which)
			{
			case 00: return new int[] { 1,2,3,4 };
			case 01: return new int[] { 1,2,4,3 };
			case 02: return new int[] { 1,3,2,4 };
			case 03: return new int[] { 1,3,4,2 };
			case 04: return new int[] { 1,4,2,3 };
			case 05: return new int[] { 1,4,3,2 };
			case 06: return new int[] { 2,1,3,4 };
			case 07: return new int[] { 2,1,4,3 };
			case 08: return new int[] { 2,3,1,4 };
			case 09: return new int[] { 2,3,4,1 };
			case 10: return new int[] { 2,4,1,3 };
			case 11: return new int[] { 2,4,3,1 };
			case 12: return new int[] { 3,1,2,4 };
			case 13: return new int[] { 3,1,4,2 };
			case 14: return new int[] { 3,2,1,4 };
			case 15: return new int[] { 3,2,4,1 };
			case 16: return new int[] { 3,4,1,2 };
			case 17: return new int[] { 3,4,2,1 };
			case 18: return new int[] { 4,1,2,3 };
			case 19: return new int[] { 4,1,3,2 };
			case 20: return new int[] { 4,2,1,3 };
			case 21: return new int[] { 4,2,3,1 };
			case 22: return new int[] { 4,3,1,2 };
			case 23: return new int[] { 4,3,2,1 };
			}
			return null;
		}
	}
}