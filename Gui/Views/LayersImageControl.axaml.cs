using Avalonia.Controls;
using Avalonia.LogicalTree;
using ImageFunctions.Gui.Models;
using ImageFunctions.Gui.ViewModels;

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
}
