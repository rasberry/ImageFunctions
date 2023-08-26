namespace ImageFunctions.Plugin;

internal static class MoreTools
{
	public static void ParalellSort<T>(IList<T> array, IComparer<T> comp = null, IProgress<double> progress = null, int? MaxDegreeOfParallelism = null)
	{
		var ps = new ParallelSort<T>(array,comp,progress);
		if (MaxDegreeOfParallelism.HasValue && MaxDegreeOfParallelism.Value > 0) {
			ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		}
		ps.Sort();
	}
}