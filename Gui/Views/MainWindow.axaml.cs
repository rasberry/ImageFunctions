using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ImageFunctions.Gui.ViewModels;
using System.Diagnostics;

namespace ImageFunctions.Gui.Views;

public partial class MainWindow : Window
{
	public MainWindow() : base()
	{
		InitializeComponent();
		OpenLayers.Click += OpenFileDialog;

		PreviewPanel.GetObservable(ScrollViewer.ViewportProperty).Subscribe((s) => {
			if (Model != null && Model.CurrentZoom != null) {
				Model.CurrentZoom.ViewPort = s;
			}
		});

		PreviewPanel.GetObservable(ScrollViewer.ExtentProperty).Subscribe((s) => {
			if (Model != null && Model.CurrentZoom != null) {
				Model.CurrentZoom.Extent = s;
			}
		});

		PreviewPanel.GetObservable(PointerMovedEvent).Subscribe((p) => {
			if (Model?.IsPickingFromPreview ?? false) {
				var pp = p.GetCurrentPoint(PreviewImage);
				Model.PreviewPointerPos = pp.Position;
				//Trace.WriteLine($"PointerMoved pp={pp.Position}");
			}
		});

		PreviewPanel.GetObservable(PointerPressedEvent).Subscribe((p) => {
			if (Model?.IsPickingFromPreview ?? false) {
				p.Handled = true;
				Model.IsPickingFromPreview = false;
			}
		});

		PreviewImage.GetObservable(PointerWheelChangedEvent).Subscribe((e) => {
			// Trace.WriteLine($"vp={PreviewPanel.Viewport}");
			e.Handled = true;
			Model?.UpdatePreviewZoomByScroll(e.Delta);
		});

		// PreviewPanel.GetObservable(ScrollViewer.OffsetProperty).Subscribe((s) => {
		// 	Trace.WriteLine($"of={s} ex={PreviewPanel.Extent} vp={PreviewPanel.Viewport}");
		// });
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
			var (s, e) = o; //tuple deconstruct
			UpdateStatusHandler(s, (PointerEventArgs)e, false);
		});
		//we don't need PointerExited becuase we're timer-hiding
		// PointerExitedEvent.Raised.Subscribe(o => {
		// 	var (s, e) = o;
		// 	UpdateStatusHandler(s, (PointerEventArgs)e, true);
		// });
	}

	void UpdateStatusHandler(object sender, PointerEventArgs args, bool isLeaving)
	{
		if(sender is not Control control) { return; }
		string text = control?.Tag?.ToString();

		if(text != null) {
			Model?.UpdateStatusText(text);
		}
	}

	IStorageProvider GetStorageProvider()
	{
		var topLevel = GetTopLevel(this);
		return topLevel?.StorageProvider;
	}

	async void OpenFileDialog(object sender, RoutedEventArgs args)
	{
		IStorageProvider sp = GetStorageProvider();
		if(sp is null) { return; }
		var filter = new List<FilePickerFileType>();
		if(Model?.SupportedReadTypes != null) {
			filter.Add(Model?.SupportedReadTypes);
		}
		filter.Add(FilePickerFileTypes.All);

		var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions() {
			Title = "Open Images",
			FileTypeFilter = filter,
			AllowMultiple = true
		});

		IStorageFile item = result.FirstOrDefault();
		if(item != null) {
			var path = item.Path.LocalPath ?? item.Path.ToString();
			Model?.LoadAndShowImage(path);
		}
	}

	/*
	public void RedrawImage(object sender, EventArgs args)
	{
		Trace.WriteLine($"{nameof(RedrawImage)}");
		var prim = PreviewPanel.FindLogicalDescendantOfType<Image>();
		prim.UpdateLayout();

		var list = LayersBox.GetLogicalDescendants();
		foreach(var node in list) {
			if (node is Image image) {
				image.UpdateLayout();
			}
		}
	}
	*/
}
