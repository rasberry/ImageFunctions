using System;

namespace ImageFunctions
{
	public static class Log
	{
		public static void Message(string m)
		{
			Console.WriteLine(m);
		}

		public static void Error(string m)
		{
			Console.Error.WriteLine("E: "+m);
		}

		public static void Debug(string m)
		{
			#if DEBUG
			Console.WriteLine("D: "+m);
			#endif
		}

		public static void ShowProgress(double start)
		{
			
		}
	}
}