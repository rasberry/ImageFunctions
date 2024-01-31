using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using ImageFunctions.Gui.ViewModels;

namespace ImageFunctions.Gui.Views;

public partial class RegisteredControl : UserControl
{
	public RegisteredControl()
	{
		InitializeComponent();
	}

	//Note: always check for null before using this e.g. Model?.
	SelectionViewModel Model {
		get {
			return DataContext as SelectionViewModel;
		}
	}

	public void OnItemSelected(object sender, SelectionChangedEventArgs args)
	{
		Model?.ItemSelected(sender,args);
	}
}
