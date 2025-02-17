using Avalonia.Controls;

namespace ImageFunctions.Gui.Views;

public partial class LayersImageControl : UserControl
{
	public LayersImageControl()
	{
		InitializeComponent();
	}

	// //Note: always check for null before using this e.g. Model?.
	// LayersImageData Model {
	// 	get {
	// 		return DataContext as LayersImageData;
	// 	}
	// }
}
