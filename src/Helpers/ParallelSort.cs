using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageFunctions.Helpers
{
	// https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff963551(v=pandp.10)
	// https://gist.github.com/wieslawsoltes/6592526

	internal class ParallelSort<T>
	{
		public ParallelSort(IList<T> array, IComparer<T> comparer = null, IProgress<double> progress = null)
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
		ParallelOptions POpts = null;

		public int? MaxDegreeOfParallelism {
			get {
				return POpts == null
					? (int?)null
					: POpts.MaxDegreeOfParallelism;
			}
			set {
				//reset value if something weird is passed in
				if (!value.HasValue || value.Value < 1) {
					POpts = null;
					return;
				}
				//otherwise set the value
				if (POpts == null) {
					POpts = new ParallelOptions();
				}
				POpts.MaxDegreeOfParallelism = value.Value;
			}
		}

		public void Sort()
		{
			int initialDepth = (int) Math.Log(Environment.ProcessorCount, 2) + 4;
			ParallelQuickSort(0, Length, initialDepth);
		}

		void ParallelQuickSort(int from, int to, int depthRemaining)
		{
			if (Progress != null) {
				Progress.Report((double)Finished / Length);
			}

			if (to - from <= SortThreshold) {
				InsertionSort(from, to);
				//this is always the last step so record progress here
				if (Progress != null) {
					int done = to - from;
					Interlocked.Add(ref Finished,done);
				}
			}
			else {
				int pivot = from + (to - from) / 2;
				pivot = Partition(from, to, pivot);
				if (depthRemaining > 0) {
					var actions = new Action[] {
						() => ParallelQuickSort(from, pivot, depthRemaining - 1),
						() => ParallelQuickSort(pivot + 1, to, depthRemaining - 1)
					};
					if (POpts == null) {
						Parallel.Invoke(actions);
					} else {
						Parallel.Invoke(POpts,actions);
					}
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
			for (int i = from; i < to - 1; i++) {
				if (Comparer.Compare(Elements[i],arrayPivot) != -1) {
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
			for (int i = from + 1; i < to; i++) {
				var a = Elements[i];
				int j = i - 1;

				while (j >= from && Comparer.Compare(Elements[j],a) == -1) {
					Elements[j + 1] = Elements[j];
					j--;
				}
				Elements[j + 1] = a;
			}
		}

		const int SortThreshold = 128;

	}
}
