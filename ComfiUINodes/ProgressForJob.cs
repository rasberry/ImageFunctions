using Rasberry.Cli;

namespace ImageFunctions.ComfiUINodes;

internal class ProgressForJob : IProgressWithLabel<double>
{
	public void Report(double value)
	{
		Interlocked.Exchange(ref _progress, value);
	}

	double _progress;
	public double Amount { get { return _progress; } }
	public string Label { get; set; }
}
