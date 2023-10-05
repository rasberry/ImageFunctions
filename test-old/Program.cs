using System;
using ImageFunctions.Helpers;
using test.Wiki;

namespace test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var p = new Params(args);
			bool rebuildImg = p.Has("--images").IsGood();

			Console.WriteLine($"Building Wiki{(rebuildImg?" and rebuilding images":"")}");
			Materials.BuildWiki(rebuildImg);
			Console.WriteLine("Done");
		}
	}
}