using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace test.Wiki
{
	// this did not work due to dll loading issues.
	public class BuildWikiTask : Task
	{
		public string ReBuildImages { get; set; }

		public override bool Execute()
		{
			bool rebuildImg = false;
			if (!String.IsNullOrWhiteSpace(ReBuildImages)) {
				rebuildImg =
					   ReBuildImages.StartsWith("y")
					|| ReBuildImages.StartsWith("Y")
					|| ReBuildImages.StartsWith("1")
					|| ReBuildImages.StartsWith("T")
					|| ReBuildImages.StartsWith("t")
				;
			}

			Console.WriteLine($"Building Wiki{(rebuildImg?" and rebuilding images":"")}");
			Materials.BuildWiki(rebuildImg);
			return true;
		}
	}

	/*
	Notes:

	https://ithrowexceptions.com/2020/08/04/implementing-and-debugging-custom-msbuild-tasks.html
	*/
}