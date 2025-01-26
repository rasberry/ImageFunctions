using Rasberry.Cli;

namespace ImageFunctions.Gui.Helpers;

internal class ProgressTracker : IProgressWithLabel<double>
{
	public void Report(double value)
	{
		Interlocked.Exchange(ref _progress, value);
		var args = new ReportEventArgs { Amount = _progress };
		OnReport?.Invoke(this, args);
	}

	double _progress;
	public double Amount { get { return _progress; } }
	public string Label { get; set; }

	public delegate void ReportEventHandler(object sender, ReportEventArgs args);
	public event ReportEventHandler OnReport;
}

internal class ReportEventArgs : EventArgs
{
	public double Amount { get; init; }
}