using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ImageFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Runtime.CompilerServices;
using ImageFunctions.Helpers;


namespace test
{
	public class Materials
	{
		static IAmTest GetTestInstance(Activity which)
		{
			switch(which)
			{
			case Activity.PixelateDetails: return new TestPixelateDetails();
			case Activity.Derivatives: return new TestDerivatives();
			case Activity.AreaSmoother: return new TestAreaSmoother();
			case Activity.AreaSmoother2: return new TestAreaSmoother2();
			case Activity.ZoomBlur: return new TestZoomBlur();
			case Activity.Swirl: return new TestSwirl();
			case Activity.Deform: return new TestDeform();
			case Activity.Encrypt: return new TestEncrypt();
			case Activity.PixelRules: return new TestPixelRules();
			case Activity.ImgDiff: return new TestImgDiff();
			case Activity.AllColors: return new TestAllColors();
			case Activity.SpearGraphic: return new TestSpearGraphic();
			case Activity.UlamSpiral: return new TestUlamSpiral();
			case Activity.GraphNet: return new TestGraphNet();
			case Activity.Maze: return new TestMaze();
			}
			return null;
		}

		public static void BuildWiki()
		{
			BuildUsage();
			BuildSideBar();
			BuildExamples();
			BuildIndividualExamples();
			BuildImages();
		}

		static void BuildUsage()
		{
			string text = Options.GetUsageText(Activity.None, Options.PickShow.FullHelp);
			string usage = string.Format(TemplateUsage,text);

			string file = Path.Combine(Helpers.WikiRoot,"usage.md");
			File.WriteAllText(file,usage);
		}

		static void BuildSideBar()
		{
			var sb = new StringBuilder();
			foreach(Activity act in Helpers.GetAllActivity())
			{
				string link = GetPageLink(act.ToString(),GetExampleLinkName(act));
				sb.AppendLine("  * "+link);
			}

			string text = string.Format(TemplateSideBar,sb.ToString());
			string file = Path.Combine(Helpers.WikiRoot,"_Sidebar.md");
			File.WriteAllText(file,text);
		}

		static void BuildExamples()
		{
			var sb = new StringBuilder();
			foreach(Activity act in Helpers.GetAllActivity())
			{
				string link = GetPageLink(act.ToString(),GetExampleLinkName(act));
				sb.AppendLine("* "+link);
			}
			string text = string.Format(TemplateExamples,sb.ToString());
			string file = Path.Combine(Helpers.WikiRoot,"examples.md");
			File.WriteAllText(file,text);
		}

		static void BuildIndividualExamples()
		{
			foreach(Activity act in Helpers.GetAllActivity())
			{
				var inst = GetTestInstance(act);
				if (inst == null) { continue; }

				var sb = new StringBuilder();
				string tbl = inst.Set == FileSet.NoneOne
					? BuildGenExamplesTable(act, inst as IAmTestNoneOne)
					: BuildExamplesTable(act, inst as IAmTestSomeOne);
				;
				sb.AppendFormat(TemplateExample, act.ToString(), tbl);
				string file = Path.Combine(Helpers.WikiRoot,GetExampleLinkName(act)+".md");
				File.WriteAllText(file,sb.ToString());
			}
		}

		//examples for activities with input images
		//row = input images
		//columns = parameters
		static string BuildExamplesTable(Activity act, IAmTestSomeOne inst)
		{
			var images = inst.GetImageNames();
			int imgLenMax = images.Max((s) => s.Enumerate<string>().Max((t) => t.Length));
			int caseCount = inst.CaseCount;
			var argsInCase = Enumerable.Range(0, caseCount)
				.Select(i => inst.GetArgs(i))
				.Select(s => s.Length < 1 ? "Default" : string.Join(' ',s))
			;
			var tbl = new WikiTable();
			var headerTxt = Enumerable.Concat(new string[] { "Image" },argsInCase);
			tbl.SetHeader(headerTxt);

			foreach (ITuple img in images) {
				var rowTxt = new List<string>();
				rowTxt.Add(TupleToLabel(img));
				for(int i=0; i<caseCount; i++) {
					string outFile = Helpers.CheckFile(act,img,i,true);
					string cell = GetImageLink(TupleToString(img),outFile,i);
					rowTxt.Add(cell);
				}
				tbl.AddRow(rowTxt);
			}

			return tbl.ToString();
		}

		//examples for generator activies (no input images)
		//row = parameters
		//columns = output image
		static string BuildGenExamplesTable(Activity act, IAmTestNoneOne inst)
		{
			int caseCount = inst.CaseCount;
			var argsInCase = Enumerable.Range(0, caseCount)
				.Select(i => inst.GetArgs(i))
				.Select(s => s.Length < 1 ? "Default" : string.Join(' ',s))
				.ToList()
			;
			var tbl = new WikiTable();
			tbl.SetHeader(new string[] { "Example", "Image" });
			for(int i=0; i<caseCount; i++) {
				string name = inst.GetOutName(i);
				string outFile = Helpers.CheckFile(act,name,i,true);
				string cell = GetImageLink(name,outFile,i);
				tbl.AddRow(new string[] { argsInCase[i], cell });
			}

			return tbl.ToString();
		}

		static string GetImageLink(string name, string link, int index)
		{
			string linkName = $"{name}-{index}";
			string linkLink = $"{link} \"{linkName}\"";
			string text = $"![{linkName}]({linkLink})";
			return text;
		}

		static string GetPageLink(string name, string link)
		{
			string text = $"[{name}]({link})";
			return text;
		}

		static string GetExampleLinkName(Activity act)
		{
			return "ex_"+act.ToString().ToLowerInvariant();
		}

		static void BuildImages()
		{
			foreach(Activity act in Helpers.GetAllActivity())
			{
				var inst = GetTestInstance(act);
				if (inst == null) { continue; }
				if (inst.Set == FileSet.NoneOne) {
					BuildImagesNoneOne(act,inst as IAmTestNoneOne);
				}
				else {
					BuildImagesSomeOne(act,inst as IAmTestSomeOne);
				}

			}
		}

		static void BuildImagesSomeOne(Activity act, IAmTestSomeOne inst)
		{
			var images = inst.GetImageNames();
			int count = inst.CaseCount;

			foreach (ITuple img in images) {
				ITuple inFile = Helpers.InFile(img);
				for(int c=0; c<count; c++) {
					string outFile = Helpers.CheckFile(act,img,c);
					//only generate if missing
					if (File.Exists(outFile)) { continue; }
					var args = Helpers.Append(inst.GetArgs(c),inFile,outFile);
					BuildImagesRun(act,args);
				}
			}
		}

		static void BuildImagesNoneOne(Activity act, IAmTestNoneOne inst)
		{
			int count = inst.CaseCount;
			for(int c=0; c<count; c++) {
				string outFile = Helpers.CheckFile(act,inst.GetOutName(c),c);
				//only generate if missing
				if (File.Exists(outFile)) { continue; }
				var args = Helpers.Append(inst.GetArgs(c),outFile);
				BuildImagesRun(act,args,inst.GetBounds(c));
			}
		}

		static void BuildImagesRun(Activity act, string[] args, Rectangle? bounds = null)
		{
			var func = Registry.Map(act); //must make a new instance each time or args get jumbled
			if (bounds != null) { func.Bounds = bounds.Value; }
			if (!func.ParseArgs(args)) {
				throw new ArgumentException("unable to parse arguments "+string.Join(' ',args));
			}
			func.Main();
		}

		//0 = usage text
		const string TemplateUsage =
			   "# Usage #"
			+"\n"
			+"\n```"
			+"\n{0}"
			+"\n```"
		;

		const string TemplateExamples =
			   "# Examples #"
			+"\n"
			+"\nExamples are categorized by Action"
			+"\n{0}"
		;
		// 0 = activity name 1 = image table
		const string TemplateExample =
			   "# {0} #"
			+"\n"
			+"\n{1}"
		;

		const string TemplateSideBar =
			   "* [Home](home)"
			+"\n* [Usage](usage)"
			+"\n* [Examples](examples)"
			+"\n{0}"
			+  "* [TODO](todo)"
		;

		static (string,string) MakeTableHeader(int imgLenMax, IEnumerable<string[]> argsInCase)
		{
			var sb1 = new StringBuilder();
			var sb2 = new StringBuilder();
			sb1.Append("| Image ");
			sb2.Append("|-------");
			if (imgLenMax > 7) {
				sb1.Append(new string(' ',imgLenMax - 7));
				sb2.Append(new string('-',imgLenMax - 7));
			}

			foreach(string[] args in argsInCase)
			{
				if (args.Length < 1) {
					sb1.Append("| Default ");
					sb2.Append("|---------");
				}
				else {
					sb1.Append('|').Append(' ');
					sb2.Append('|').Append('-');
					foreach(string a in args)
					{
						sb1.Append(a);
						sb2.Append(new string('-',a.Length));
						sb1.Append(' ');
						sb2.Append('-');
					}
				}
			}
			sb1.Append('|');
			sb2.Append('|');

			return (sb1.ToString(),sb2.ToString());
		}

		static string MakeTableRow(Activity which, ITuple images, int imgLenMax, int argCount)
		{
			int w = (int)which;
			var sb = new StringBuilder();
			string topLabel = (string)images[0];
			sb.Append('|').Append(TupleToLabel(images));
			sb.Append(new string(' ',Math.Max(7,imgLenMax) - topLabel.Length));
			sb.Append('|');
			for(int i=0; i<argCount; i++)
			{
				string outFile = Helpers.CheckFile(which,images,i,true);
				sb.Append(GetImageLink(TupleToString(images),outFile,i));
			}
			return sb.AppendLine().ToString();
		}

		static string TupleToLabel(ITuple tuple)
		{
			return string.Join("<br/>",tuple.Enumerate<string>());
		}

		static string TupleToString(ITuple tuple)
		{
			return string.Join('-',tuple.Enumerate<string>());
		}
	}
}
