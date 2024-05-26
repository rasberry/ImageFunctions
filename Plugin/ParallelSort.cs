namespace ImageFunctions.Plugin;

// https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff963551(v=pandp.10)
// https://gist.github.com/wieslawsoltes/6592526

internal class QuickSort<T>
{
	public QuickSort(IList<T> array, IComparer<T> comparer = null, IProgress<double> progress = null)
	{
		Elements = array;
		Length = array.Count;
		Comparer = comparer != null
			? comparer
			: Comparer<T>.Default;
		Progress = progress;
	}

	IList<T> Elements;
	IComparer<T> Comparer;
	IProgress<double> Progress;
	int Finished = 0;
	int Length;

	public int? MaxDegreeOfParallelism { get; set; }

	public void Sort()
	{
		int initialDepth = (int)Math.Log(Environment.ProcessorCount, 2) + 4;
		ParallelQuickSort(0, Length, initialDepth);
	}

	void ParallelQuickSort(int from, int to, int depthRemaining)
	{
		var options = new ParallelOptions();
		if(MaxDegreeOfParallelism.HasValue) {
			options.MaxDegreeOfParallelism = MaxDegreeOfParallelism.Value;
		}

		if(Progress != null) {
			Progress.Report((double)Finished / Length);
		}

		if(to - from <= SortThreshold) {
			InsertionSort(from, to);
			//this is always the last step so record progress here
			if(Progress != null) {
				int done = to - from;
				Interlocked.Add(ref Finished, done);
			}
		}
		else {
			int pivot = from + (to - from) / 2;
			pivot = Partition(from, to, pivot);
			if(depthRemaining > 0) {
				var actions = new Action[] {
					() => ParallelQuickSort(from, pivot, depthRemaining - 1),
					() => ParallelQuickSort(pivot + 1, to, depthRemaining - 1)
				};
				Parallel.Invoke(options, actions);
			}
			else {
				ParallelQuickSort(from, pivot, 0);
				ParallelQuickSort(pivot + 1, to, 0);
			}
		}
	}

	int Partition(int from, int to, int pivot)
	{
		var arrayPivot = Elements[pivot];
		Swap(pivot, to - 1);
		var newPivot = from;
		for(int i = from; i < to - 1; i++) {
			if(Comparer.Compare(Elements[i], arrayPivot) != -1) {
				Swap(newPivot, i);
				newPivot++;
			}
		}
		Swap(newPivot, to - 1);
		return newPivot;
	}

	void Swap(int i, int j)
	{
		var temp = Elements[i];
		Elements[i] = Elements[j];
		Elements[j] = temp;
	}

	void InsertionSort(int from, int to)
	{
		for(int i = from + 1; i < to; i++) {
			var a = Elements[i];
			int j = i - 1;

			while(j >= from && Comparer.Compare(Elements[j], a) == -1) {
				Elements[j + 1] = Elements[j];
				j--;
			}
			Elements[j + 1] = a;
		}
	}

	const int SortThreshold = 128;
}

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

	IList<T> Data;
	IComparer<T> Compare;
	IProgress<double> Progress;
}
