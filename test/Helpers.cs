using System;
using System.IO;

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
	}
}
