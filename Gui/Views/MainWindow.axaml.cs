using System.Diagnostics;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using ImageFunctions.Gui.ViewModels;
using ReactiveUI;

namespace ImageFunctions.Gui.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	//Note: always check for null before using this e.g. Model?.
	MainWindowViewModel Model {
		get {
			return DataContext as MainWindowViewModel;
		}
	}

	protected override void OnInitialized()
	{
		//setup global even handlers for mouse enter/exit events
		PointerEnteredEvent.Raised.Subscribe(o => {
			var (s,e) = o; //tuple deconstruct
			UpdateStatusHandler(s, (PointerEventArgs)e, false);
		});
		PointerExitedEvent.Raised.Subscribe(o => {
			var (s,e) = o;
			UpdateStatusHandler(s, (PointerEventArgs)e, true);
		});
	}

	void UpdateStatusHandler(object sender, PointerEventArgs args, bool isLeaving)
	{
		if (sender is not Button button) { return; }
		string text = button?.Tag?.ToString();

		if (text != null) {
			Model?.UpdateStatusText(text, isLeaving);
		}
	}
}