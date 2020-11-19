using System;
using test.Wiki;

namespace test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			bool rebuildImg = false;
			if (args.Length > 0) {
				rebuildImg = true;
			}

			Console.WriteLine($"Building Wiki{(rebuildImg?" and rebuilding images":"")}");
			Materials.BuildWiki(rebuildImg);
			Console.WriteLine("Done");
		}
	}
}