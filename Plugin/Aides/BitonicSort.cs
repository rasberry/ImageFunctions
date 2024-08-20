namespace ImageFunctions.Plugin.Aides;

// https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff963551(v=pandp.10)
// https://gist.github.com/wieslawsoltes/6592526

internal class BitonicSort<T>
{
	public BitonicSort(IList<T> array, IComparer<T> comparer = null, IProgress<double> progress = null)
	{
		Data = array;
		Compare = comparer;
		Progress = progress;
	}

	public int? MaxDegreeOfParallelism { get; set; }

	public void Sort()
	{
		var options = new ParallelOptions();
		if(MaxDegreeOfParallelism.HasValue) {
			options.MaxDegreeOfParallelism = MaxDegreeOfParallelism.Value;
		}

		//number of iterations is exactly (log2(n) + 2) * log2(n) / 2
		int l = System.Numerics.BitOperations.Log2((uint)Data.Count) + 1;
		int t = (l + 1) * l / 2;
		int c = 0;

		// Iterate k as if the array size were rounded up to the nearest power of two.
		int n = Data.Count;
		for(int k = 2; k / 2 < n; k *= 2) {
			Parallel.For(0, n, options, (i) => {
				CompareExchange(Data, i, i ^ (k - 1));
			});
			for(int j = k / 2; j > 0; j /= 2) {
				Parallel.For(0, n, options, (i) => {
					CompareExchange(Data, i, i ^ j);
				});
				Progress?.Report(++c / (double)t);
				//Console.WriteLine($"c={++c} k={k} j={j}");
			}
		}
	}

	void CompareExchange(IList<T> arr, int i, int j)
	{
		int comp = Compare.Compare(arr[i], arr[j]);
		if(i < j && j < arr.Count && comp > 0) {
			(arr[i], arr[j]) = (arr[j], arr[i]);
		}
	}

	readonly IList<T> Data;
	readonly IComparer<T> Compare;
	readonly IProgress<double> Progress;
}
