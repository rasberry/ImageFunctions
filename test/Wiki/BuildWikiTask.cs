using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace test.Wiki
{
	public class BuildWikiTask : Task
	{
		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.High,$"Building Wiki{(RebuildImages?" and rebuilding images":"")}");
			Materials.BuildWiki(RebuildImages);
			Log.LogMessage(MessageImportance.High,"Done");

			return true;
		}

		public bool RebuildImages { get; set; }
	}
}