namespace ImageFunctions.ComfiUINodes;

internal class ProgressForJob : IProgress<double>
{
	public void Report(double value)
	{
		Interlocked.Exchange(ref _progress, value);
	}

	double _progress;
	public double Amount { get { return _progress; } }
}
