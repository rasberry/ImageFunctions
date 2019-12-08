using System;
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
		public static void BuildWiki()
		{
			BuildUsage();
			BuildExamples();
			BuildImages();
		}

		static void BuildUsage()
		{
			string text = Options.GetUsageText(Activity.None,true,false);
			string usage = string.Format(TemplateUsage,text);

			string file = Path.Combine(Helpers.WikiRoot,"usage.md");
			File.WriteAllText(file,usage);
		}

		static void BuildExamples()
		{
			var sb = new StringBuilder();
			sb.Append(TemplateExamplesHeader);

			foreach(Activity act in OptionsHelpers.EnumAll<Activity>())
			{
				var inst = GetTestInstance(act);
				if (inst == null) { continue; }

				string tbl = BuildExamplesTable(act, inst);
				sb.AppendFormat(TemplateExample, act.ToString(), tbl);
			}

			string file = Path.Combine(Helpers.WikiRoot,"examples.md");
			File.WriteAllText(file,sb.ToString());
		}

		static string BuildExamplesTable(Activity act, IAmTest inst)
		{
			var tbl = new StringBuilder();
			var images = inst.GetImageNames();
			int imgLenMax = images.Max((s) => s.Enumerate<string>().Max((t) => t.Length));
			int caseCount = inst.CaseCount;
			var argsInCase = Enumerable.Range(0, caseCount)
				.Select((i) => inst.GetArgs(i));

			(string th1, string th2) = MakeTableHeader(imgLenMax, argsInCase);
			tbl.AppendLine(th1).AppendLine(th2);

			foreach (ITuple img in images) {
				string row = MakeTableRow(act, img, imgLenMax, caseCount);
				tbl.Append(row);
			}

			return tbl.ToString();
		}

		static void BuildImages()
		{
			foreach(Activity act in OptionsHelpers.EnumAll<Activity>())
			{
				var inst = GetTestInstance(act);
				if (inst == null) { continue; }

				var images = inst.GetImageNames();
				int count = inst.CaseCount;

				foreach (ITuple img in images)
				{
					ITuple inFile = Helpers.InFile(img);
					for(int c=0; c<count; c++)
					{
						string outFile = Helpers.CheckFile(act,img,c);
						var args = Helpers.Append(inst.GetArgs(c),inFile,outFile);
						// Helpers.Debug("act="+act+" img="+img+" c="+c+" args = "+string.Join(' ',args));

						//only generate if missing
						if (!File.Exists(outFile)) {
							var func = Registry.Map(act); //must make a new instance each time or args get jumbled
							if (!func.ParseArgs(args)) {
								throw new ArgumentException("unable to parse arguments "+string.Join(' ',args));
							}
							func.Main();
						}
					}
				}
			}
		}

		//0 = usage text
		const string TemplateUsage =
			   "# Usage #"
			+"\n"
			+"\n```"
			+"\n{0}"
			+"\n```"
		;

		const string TemplateExamplesHeader =
			   "# Examples #"
			+"\n"
			+"\nExamples are categorized by Action"
			+"\n"
		;
		// 0 = activity name 1 = image table
		const string TemplateExample =
			"<details><summary>{0}</summary>"
			+"\n"
			+"\n{1}"
			+"\n</details>"
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
				sb.AppendFormat("![{0}-{2}]({1} \"{0}-{2}\")|"
					,TupleToString(images),outFile,i);
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
			}
			return null;
		}
	}
}
