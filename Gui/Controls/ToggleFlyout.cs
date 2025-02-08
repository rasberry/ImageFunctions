using Avalonia.Controls;
using System.Diagnostics;

namespace ImageFunctions.Gui.Controls;

public class ToggleFlyout : Flyout
{
	public ToggleFlyout() : base()
	{
		this.Closing += (s,e) => {
			Trace.WriteLine($"ToggleFlyout Closing {s?.GetType().FullName}");
			e.Cancel = true;
		};
	}

	// protected override bool HideCore(bool canCancel = true)
	// {
	// 	Trace.WriteLine($"ToggleFlyout c={canCancel}");
	// 	return base.HideCore(true);
	// }
}