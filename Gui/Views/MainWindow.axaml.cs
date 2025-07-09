using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ImageFunctions.Gui.ViewModels;

namespace ImageFunctions.Gui.Views;

public partial class MainWindow : Window
{
	public MainWindow() : base()
	{
		InitializeComponent();

		//Using click because not sure why PointerPressed doesn't work
		OpenLayers.Click += OpenFileDialog;
		SavePreview.Click += (s, e) => SaveFileDialog(e, false);
		SaveStack.Click += (s, e) => SaveFileDialog(e, true);

		PreviewPanel.GetObservable(ScrollViewer.ViewportProperty).Subscribe((s) => {
			if(Model != null && Model.CurrentZoom != null) {
				Model.CurrentZoom.ViewPort = s;
			}
		});

		PreviewPanel.GetObservable(ScrollViewer.ExtentProperty).Subscribe((s) => {
			if(Model != null && Model.CurrentZoom != null) {
				Model.CurrentZoom.Extent = s;
			}
		});

		PreviewPanel.GetObservable(PointerMovedEvent).Subscribe((p) => {
			if(Model?.IsPickingFromPreview ?? false) {
				var pp = p.GetCurrentPoint(PreviewImage);
				Model.PreviewPointerPos = pp.Position;
				//Trace.WriteLine($"PointerMoved pp={pp.Position}");
			}
		});

		PreviewPanel.GetObservable(PointerPressedEvent).Subscribe((p) => {
			if(Model?.IsPickingFromPreview ?? false) {
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

		//TODO if multiple selected we should load all of them
		using IStorageFile item = result.FirstOrDefault();
		if(item != null) {
			var path = item.Path.LocalPath ?? item.Path.ToString();
			Model?.LoadAndShowImage(path);
		}
	}

	async void SaveFileDialog(RoutedEventArgs args, bool doSaveStack)
	{
		IStorageProvider sp = GetStorageProvider();
		if(sp is null) { return; }
		var filter = new List<FilePickerFileType>();
		if(Model?.SupportedWriteTypes != null) {
			filter.Add(Model?.SupportedWriteTypes);
		}
		filter.Add(FilePickerFileTypes.All);

		var result = await sp.SaveFilePickerAsync(new FilePickerSaveOptions() {
			Title = "Open Images",
			FileTypeChoices = filter,
			DefaultExtension = filter.First().ToString(),
			ShowOverwritePrompt = true
		});

		var path = result.Path.LocalPath ?? result.Path.ToString();
		var format = Path.GetExtension(path);
		Model?.SaveImage(path, format, doSaveStack);
	}
}
