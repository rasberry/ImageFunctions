using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using ImageFunctions.Gui.ViewModels;
using ReactiveUI;

namespace ImageFunctions.Gui.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		//TODO-20240126 don't know why I can't use Button.PointerEntered on parent nodes but it won't compile
		foreach(var node in Root.GetLogicalDescendants()) {
			if (node is Button button) {
				button.PointerEntered += UpdateStatusOnEnter;
				button.PointerExited += UpdateStatusOnExit;
			}
		}
	}

	//Note: always check for null before using this e.g. Model?.
	MainWindowViewModel Model {
		get {
			return DataContext as MainWindowViewModel;
		}
	}

	public void UpdateStatusOnEnter(object sender, Avalonia.Input.PointerEventArgs args)
	{
		UpdateStatusHandler(sender, false);
	}
	public void UpdateStatusOnExit(object sender, Avalonia.Input.PointerEventArgs args)
	{
		UpdateStatusHandler(sender, true);
	}

	void UpdateStatusHandler(object sender, bool isExit)
	{
		if (sender is not Button button) { return; }
		string text = button?.Tag?.ToString();

		if (text != null) {
			Model?.UpdateStatusText(text, isExit);
		}
	}
}