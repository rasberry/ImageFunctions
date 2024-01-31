using System.Diagnostics;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
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

	protected override void OnInitialized()
	{
		/*
		this.AddHandler(PointerEnteredEvent, (s,e) => {
			Trace.WriteLine($"0 PointerEntered {e.Source.GetType().FullName}");
		}, Avalonia.Interactivity.RoutingStrategies.Bubble);

		this.AddHandler(PointerExitedEvent, (s,e) => {
			Trace.WriteLine($"0 PointerExited {e.Source.GetType().FullName}");
		}, Avalonia.Interactivity.RoutingStrategies.Bubble);

		this.AddHandler(ListBox.SelectionChangedEvent, (s,e) => {
			Trace.WriteLine($"0 SelectionChanged {e.Source.GetType().FullName}");
		}, Avalonia.Interactivity.RoutingStrategies.Bubble);
		*/

		//TODO-20240126 don't know why I can't use Button.PointerEntered on parent nodes but it won't compile
		foreach(var node in this.GetLogicalDescendants()) {
			//Trace.WriteLine($"0 Found {node.GetType().FullName}");
			if (node is Button button) {
				button.PointerEntered += UpdateStatusOnEnter;
				button.PointerExited += UpdateStatusOnExit;
			}
			/*
			else if (node is ContentControl cc) {
				Trace.WriteLine($"A Found {node.GetType().FullName} {cc.IsInitialized}");
				cc.TemplateApplied += OnContentControlLoad;
			}
			*/
		}
	}

	/*
	void OnContentControlLoad(object sender, TemplateAppliedEventArgs args)
	{
		var root = (ContentControl)args.Source;
		Trace.WriteLine($"1 Found {root.GetType().FullName}");
		foreach(var node in root.GetLogicalDescendants()) {
			Trace.WriteLine($"2 Found {node.GetType().FullName}");
			if (node is ListBox box) {
				Trace.WriteLine($"3 Found {node.GetType().FullName}");
				//if (!box.Classes.Contains("RegisteredSelectionBox")) { continue; }
				//box.SelectionChanged += SelectionChangedHandler;
			}
		};
	}
	*/

	//Note: always check for null before using this e.g. Model?.
	MainWindowViewModel Model {
		get {
			return DataContext as MainWindowViewModel;
		}
	}

	public void UpdateStatusOnEnter(object sender, PointerEventArgs args)
	{
		UpdateStatusHandler(sender, false);
	}
	public void UpdateStatusOnExit(object sender, PointerEventArgs args)
	{
		UpdateStatusHandler(sender, true);
	}

	void UpdateStatusHandler(object sender, bool isLeaving)
	{
		if (sender is not Button button) { return; }
		string text = button?.Tag?.ToString();

		if (text != null) {
			Model?.UpdateStatusText(text, isLeaving);
		}
	}

	void SelectionChangedHandler(object sender, SelectionChangedEventArgs args)
	{
		Trace.WriteLine("selected something!");
	}
}