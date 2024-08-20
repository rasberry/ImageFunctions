namespace ImageFunctions.Plugin.Aides;

internal static class MoreAide
{
	/// <summary>
	/// Sorts a list using a multi-threaded sort. Seems to work best on machines with 4+ cores
	/// </summary>
	/// <typeparam name="T">Generic Type parameter</typeparam>
	/// <param name="array">The IList<T> of items to sort</param>
	/// <param name="comp">Comparer function for T</param>
	/// <param name="progress">optional progress object</param>
	/// <param name="MaxDegreeOfParallelism">Maximum number of threads to allow</param>
	public static void ParallelSort<T>(IList<T> array, IComparer<T> comp = null, IProgress<double> progress = null, int? MaxDegreeOfParallelism = null)
	{
		//var ps = new QuickSort<T>(array,comp,progress);
		//ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		//ps.Sort();

		var ps = new BitonicSort<T>(array, comp, progress);
		ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		ps.Sort();
	}

	/// <summary>
	/// Wrapper for Parallel.For that includes progress
	/// </summary>
	/// <param name="max">The total number of iterations</param>
	/// <param name="callback">The callsback is called on each iteration</param>
	/// <param name="progress">Optional progress object</param>
	public static void ThreadRun(int max, Action<int> callback, int? maxThreads, IProgress<double> progress = null)
	{
		int done = 0;
		ParallelOptions po = new();
		if(maxThreads.HasValue) {
			po.MaxDegreeOfParallelism = maxThreads.Value < 1 ? 1 : maxThreads.Value;
		};
		Parallel.For(0, max, po, num => {
			Interlocked.Add(ref done, 1);
			progress?.Report((double)done / max);
			callback(num);
		});
	}
}
