using Avalonia.Controls;
using ImageFunctions.Gui.ViewModels;
using System.Diagnostics;

namespace ImageFunctions.Gui.Views;

public partial class LayersImageControl : UserControl
{
	public LayersImageControl()
	{
		InitializeComponent();
		//this.AttachedToLogicalTree += (s,e) => {
		//	Model?.CheckUpDownEnabled();
		//};
	}

	//Note: always check for null before using this e.g. Model?.
	LayersImageData Model {
		get {
			return DataContext as LayersImageData;
		}
	}

	public void TestClick(object sender, Avalonia.Interactivity.RoutedEventArgs args)
	{
		Trace.WriteLine($"TestClick: {sender.GetHashCode()} {sender.GetType().FullName} {args.RoutedEvent.Name}");
	}
}
