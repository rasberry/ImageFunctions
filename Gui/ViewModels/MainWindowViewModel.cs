using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace ImageFunctions.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public MainWindowViewModel()
	{
		var imageStorage = new ImageStorage(ConvertCanvasToRgba8888);
		LayersImageList = imageStorage.Bitmaps;
		Layers = imageStorage.Layers;

		RxApp.MainThreadScheduler.Schedule(LoadData);
		LayersImageList.CollectionChanged += OnLayersCollectionChange;
	}

	static readonly TimeSpan WarningTimeout = TimeSpan.FromSeconds(10.0);
	static readonly TimeSpan CommandTimeout = TimeSpan.FromHours(24);
	static readonly TimeSpan StatusTextTimeout = TimeSpan.FromSeconds(2);
	static readonly Vector StandardDpi = new(96.0, 96.0); //TODO is there a way to get this from the system ?
	static readonly int BytesPerPixel = PixelFormat.Rgba8888.BitsPerPixel / 8;

	void LoadData()
	{
		RegisteredControlList = new();
		var reg = Program.Register;
		foreach(var ns in reg.Spaces()) {
			var svm = GetSelectionViewModelForNameSpace(ns);
			RegisteredControlList.Add(svm);
		}

		// var functionReg = new FunctionRegister(Program.Register);
		// RegFunctionItems = AddTreeNodeFromRegistered(SelectionKind.Functions, functionReg, (reg, name) => {
		// 	return new SelectionItem { Name = name };
		// }, OnFunctionSelected);

		// var colorReg = new ColorRegister(Program.Register);
		// RegColorItems = AddTreeNodeFromRegistered(SelectionKind.Colors, colorReg, (reg, name) => {
		// 	return new SelectionItemColor {
		// 		Name = name,
		// 		Color = ConvertColor(name, colorReg)
		// 	};
		// }, OnSomethingSelected);

		// var engineReg = new EngineRegister(Program.Register);
		// RegEngineItems = AddTreeNodeFromRegistered(SelectionKind.Engines, engineReg, (reg, name) => {
		// 	return new SelectionItem { Name = name };
		// }, OnEngineSelected);

		// var metricReg = new MetricRegister(Program.Register);
		// RegMetricItems = AddTreeNodeFromRegistered(SelectionKind.Metrics, metricReg, (reg, name) => {
		// 	return new SelectionItem { Name = name };
		// }, OnSomethingSelected);

		// var samplerReg = new SamplerRegister(Program.Register);
		// RegSamplerItems = AddTreeNodeFromRegistered(SelectionKind.Samplers, samplerReg, (reg, name) => {
		// 	return new SelectionItem { Name = name };
		// }, OnSomethingSelected);
	}

	SelectionViewModel GetSelectionViewModelForNameSpace(string ns)
	{
		var svm = ns switch {

			FunctionRegister.NS => AddTreeNodeFromRegistered(ns, (reg, item) => {
				return new SelectionItem { Name = item.Name, NameSpace = ns };
			}, OnFunctionSelected),

			ColorRegister.NS => AddTreeNodeFromRegistered(ns, (reg, item) => {
				var colorItem = reg.Get<ColorRGBA>(ns, item.Name);
				return new SelectionItemColor {
					Name = item.Name,
					NameSpace = ns,
					Color = ConvertColor(colorItem)
				};
			}, OnSomethingSelected),

			EngineRegister.NS => AddTreeNodeFromRegistered(ns, (reg, item) => {
				return new SelectionItem { Name = item.Name, NameSpace = ns };
			}, OnEngineSelected),

			_ => AddTreeNodeFromRegistered(ns, (reg, item) => {
				return new SelectionItem { Name = item.Name, NameSpace = ns };
			}, OnSomethingSelected),
		};

		return svm;
	}

	public ILayers Layers { get; init; }
	public ObservableStackList<LayersImageData> LayersImageList { get; init; }

	SelectionViewModel AddTreeNodeFromRegistered(string @namespace,
		Func<IRegister, INameSpaceName, SelectionItem> filler,
		Action<SelectionItem> selectionHandler
	)
	{
		var reg = Program.Register;
		var items = new ObservableCollection<SelectionItem>();
		var nsItems = reg.All(@namespace).OrderBy(n => n.Name);
		var def = reg.Default(@namespace);

		SelectionItem selected = null;
		foreach(var c in nsItems) {
			var item = filler(Program.Register, c);
			items.Add(item);
			if (def != null && item.Name.EqualsIC(def)) {
				selected = item;
			}
		}
		var sel = new SelectionViewModel {
			NameSpace = @namespace,
			Items = items
		};
		if (selected != null) {
			sel.Selected = selected;
		}

		sel.WhenAnyValue(p => p.Selected)
			.Subscribe(selectionHandler);

		return sel;
	}

	public ObservableCollection<SelectionViewModel> RegisteredControlList { get; private set; }

	IRegisteredItem<FunctionSpawner> RegFunction;
	void OnFunctionSelected(SelectionItem item)
	{
		if(item == null) { return; }
		var reg = new FunctionRegister(Program.Register);
		if(!reg.Try(item.Name, out RegFunction)) {
			//Trace.WriteLine(GuiNote.RegisteredItemWasNotFound(item.Name));
			return;
		}

		var task = Models.SingleTasks.GetOrMake(nameof(OnFunctionSelected), job);
		_ = task?.Run(); //fire and forget

		void job(CancellationToken token)
		{
			var func = RegFunction?.Item.Invoke(Program.Register, null, null);
			token.ThrowIfCancellationRequested();

			var opts = func.Options;
			var sb = new StringBuilder();
			func?.Options.Usage(sb, Program.Register);
			token.ThrowIfCancellationRequested();

			Dispatcher.UIThread.Post(() => {
				UsageText = sb.ToString();
				if (opts is IUsageProvider iup) {
					RePopulateInputControls(iup, token);
				}
			});
		}
	}

	EngineWrapper RegEngine;
	void OnEngineSelected(SelectionItem item)
	{
		if(item == null) { return; }
		var reg = new EngineRegister(Program.Register);
		if(!reg.Try(item.Name, out var engItem)) {
			//Trace.WriteLine(GuiNote.RegisteredItemWasNotFound(item.Name));
			return;
		}
		RegEngine = new EngineWrapper(engItem);

		var task = SingleTasks.GetOrMake(nameof(OnEngineSelected), job);
		_ = task?.Run(); //fire and forget

		void job(CancellationToken token)
		{
			var eng = RegEngine;
			if(eng == null) { return; }

			var list = eng.Formats();
			token.ThrowIfCancellationRequested();

			var readPatterns = new List<string>();
			var writePatterns = new List<string>();
			var readMime = new List<string>();
			var writeMime = new List<string>();
			//TODO apple formats.. https://developer.apple.com/documentation/uniformtypeidentifiers/system-declared_uniform_type_identifiers

			foreach(var f in list) {
				token.ThrowIfCancellationRequested();
				bool r = f.CanRead, w = f.CanWrite;
				bool m = string.IsNullOrEmpty(f.MimeType);
				if(r) { readPatterns.Add("*" + f.BestExtension); }
				if(r & m) { readMime.Add(f.MimeType); }
				if(w) { writePatterns.Add("*" + f.BestExtension); }
				if(w & m) { writeMime.Add(f.MimeType); }
			}
			SupportedReadTypes = new FilePickerFileType("Readable Formats") {
				Patterns = readPatterns,
				MimeTypes = readMime
			};
			SupportedWriteTypes = new FilePickerFileType("Writeable Formats") {
				Patterns = writePatterns,
				MimeTypes = writeMime
			};
		}
	}

	void OnSomethingSelected(SelectionItem item)
	{
		Log.Debug($"Something selected {item?.Name}");
	}

	static Avalonia.Media.SolidColorBrush ConvertColor(IRegisteredItem<ColorRGBA> item)
	{
		var c = item.Item;
		var ac = Avalonia.Media.Color.FromArgb(
			(byte)(c.A * 255.0),
			(byte)(c.R * 255.0),
			(byte)(c.G * 255.0),
			(byte)(c.B * 255.0)
		);
		return new Avalonia.Media.SolidColorBrush(ac);
	}

	public FilePickerFileType SupportedReadTypes { get; private set; }
	public FilePickerFileType SupportedWriteTypes { get; private set; }

	string _statusText = $"Welcome to {nameof(ImageFunctions)}"; //TODO add version
	public string StatusText {
		get => _statusText;
		set => this.RaiseAndSetIfChanged(ref _statusText, value);
	}

	string _commandText = "";
	public string CommandText {
		get => _commandText;
		set => this.RaiseAndSetIfChanged(ref _commandText, value);
	}

	string _usageText = "";
	public string UsageText {
		get => _usageText;
		set => this.RaiseAndSetIfChanged(ref _usageText, value);
	}

	public void ToggleThemeClick()
	{
		var app = Application.Current;
		if(app is not null) {
			var theme = app.ActualThemeVariant;
			app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
		}
	}

	// The behavior shows the text as long as the control is still under the pointer
	// but wait before hiding the text after the pointer leaves
	public void UpdateStatusText(string text, bool startTimer, TimeSpan? timeout = null)
	{
		//Trace.WriteLine($"UpdateStatusText T:'{text}' E:{(startTimer?"Y":"N")} T:{timeout.GetValueOrDefault().TotalMilliseconds}");
		if(StatusTextTimer == null) {
			StatusTextTimer = new() {
				AutoReset = false,
				Interval = StatusTextTimeout.TotalMilliseconds
			};
			//this clears the status after some time
			StatusTextTimer.Elapsed += (s, e) => UpdateStatusText("", false);
		}

		StatusText = text;

		if(startTimer) {
			if(timeout != null) {
				//Trace.WriteLine($"{nameof(UpdateStatusText)} timeout set {timeout.Value.TotalMilliseconds}");
				StatusTextTimer.Interval = timeout.Value.TotalMilliseconds;
			}
			StatusTextTimer.Start();
		}
		else {
			StatusTextTimer.Stop();
			StatusTextTimer.Interval = StatusTextTimeout.TotalMilliseconds;
		}
	}
	System.Timers.Timer StatusTextTimer = null;

	public Rect PreviewRectangle { get; set; }

	void UpdateLayerImageButtons(int newIx, int oldIx)
	{
		//Trace.WriteLine($"{nameof(UpdateLayerImageButtons)} {newIx} {oldIx} {Layers.Count}");
		UpdateLayerImageButtonsAddIndex(newIx);
		UpdateLayerImageButtonsAddIndex(oldIx);
	}

	void UpdateLayerImageButtonsAddIndex(int ix)
	{
		if(ix < 0 || ix >= Layers.Count) { return; }
		LayersImageList[ix].CheckUpDownEnabled();
		if(ix - 1 >= 0) {
			LayersImageList[ix - 1].CheckUpDownEnabled();
		}
		if(ix + 1 < Layers.Count) {
			LayersImageList[ix + 1].CheckUpDownEnabled();
		}
	}

	void OnLayersCollectionChange(object sender, NotifyCollectionChangedEventArgs args)
	{
		UpdateLayerImageButtons(args.NewStartingIndex, args.OldStartingIndex);
	}

	/*
	Bitmap _primaryImageSource;
	public Bitmap PrimaryImageSource {
		get => _primaryImageSource;
		set => this.RaiseAndSetIfChanged(ref _primaryImageSource, value);
	}
	public Rect PreviewRectangle { get; set; }

	void OnLayersCollectionChange(object sender, NotifyCollectionChangedEventArgs args)
	{
		Trace.WriteLine($"{nameof(OnLayersCollectionChange)} {args.Action} {args.NewStartingIndex} {args.OldStartingIndex}");

		//we only care if the first image was changed
		bool isNotable = args.OldStartingIndex == 0 || args.NewStartingIndex == 0;
		if (!isNotable) { return; }

		var roTask = SingleTasks.Get(nameof(PrimaryImageSource));
		Trace.WriteLine($"{nameof(OnLayersCollectionChange)} R:{roTask?.IsRunning}");

		var task = SingleTasks.GetOrMake(nameof(PrimaryImageSource),job);
		_ = task.Run();

		void job(CancellationToken token) {
			//Trace.WriteLine($"{nameof(OnLayersCollectionChange)} started job");
			if (Layers.Count < 1) { return; }
			token.ThrowIfCancellationRequested();
			var item = Layers[Layers.Count - 1]; //the 'Top' of the stack is the last image

			Trace.WriteLine($"Updating Primary image {item.Canvas.Width}x{item.Canvas.Height}");
			var orig = PrimaryImageSource;
			PrimaryImageSource = ConvertCanvasToRgba8888(item.Canvas);
			orig?.Dispose();
		}
	}
	*/

	void OnPrimaryImageAreaChange(Rect previewSizeBounds)
	{
		//TODO scroll / zoom updates
	}

	public void LoadAndShowImage(string fileName)
	{
		if(RegEngine == null) {
			var txt = GuiNote.WarningMustBeSelected("engine");
			UpdateStatusText(txt, true, WarningTimeout);
			return;
		}

		//Trace.WriteLine($"{nameof(LoadAndShowImage)} {fileName}");
		RegEngine.LoadImage(Layers, fileName);
	}

	Bitmap ConvertCanvasToRgba8888(ICanvas canvas)
	{
		var previewBounds = RectSizeToPixels(PreviewRectangle, StandardDpi); //TODO this is definitely wrong
		var imgBounds = new Rect(0, 0, canvas.Width, canvas.Height);
		var workBounds = imgBounds.Intersect(previewBounds);

		//Trace.WriteLine($"Rgba8888 P:{previewBounds} I:{imgBounds} W:{workBounds}");
		if(workBounds.Width < 1 || workBounds.Height < 1) {
			return null;
		}

		int wWidth = (int)workBounds.Width;
		int wHeight = (int)workBounds.Height;
		int wLeft = (int)workBounds.Left;
		int wTop = (int)workBounds.Top;
		int wRight = (int)workBounds.Right;
		int wBottom = (int)workBounds.Bottom;

		byte[] data = new byte[wWidth * wHeight * BytesPerPixel];

		//Trace.WriteLine($"Rgba8888 {wWidth} {wHeight} {wTop} {wBottom} {wLeft} {wRight}");
		int dataOffset = 0;
		for(int y = wTop; y < wBottom; y++) {
			for(int x = wLeft; x < wRight; x++) {
				var pix = canvas[x, y];
				data[dataOffset + 0] = (byte)(pix.R * 255.0);
				data[dataOffset + 1] = (byte)(pix.G * 255.0);
				data[dataOffset + 2] = (byte)(pix.B * 255.0);
				data[dataOffset + 3] = (byte)(pix.A * 255.0);
				dataOffset += BytesPerPixel;
			}
		}

		var bitmap = new WriteableBitmap(
			new PixelSize(wWidth, wHeight),
			StandardDpi,
			PixelFormat.Rgba8888,
			AlphaFormat.Unpremul
		);

		using(var buffer = bitmap.Lock()) {
			System.Runtime.InteropServices.Marshal.Copy(data, 0, buffer.Address, data.Length);
		}

		return bitmap;
	}

	static Rect RectSizeToPixels(Rect size, Vector dpi)
	{
		Size one = new(size.Left, size.Top);
		Size two = new(size.Width, size.Height);
		var pone = PixelSize.FromSize(one, dpi);
		var ptwo = PixelSize.FromSize(two, dpi);
		return new Rect(pone.Width, pone.Height, ptwo.Width, ptwo.Height);
	}

	public void RunCommand()
	{
		Trace.WriteLine(nameof(RunCommand));
		if(RegFunction == null) {
			var txt = GuiNote.WarningMustBeSelected("function");
			UpdateStatusText(txt, true, WarningTimeout);
			return;
		}
		if(RegEngine == null) {
			var txt = GuiNote.WarningMustBeSelected("engine");
			UpdateStatusText(txt, true, WarningTimeout);
			return;
		}

		Trace.WriteLine($"{nameof(RunCommand)} 2");
		var task = SingleTasks.GetOrMake(nameof(RunCommand), job, CommandTimeout);
		_ = task.Run();

		void job(CancellationToken token)
		{
			//Trace.WriteLine($"{nameof(RunCommand)} 3");
			token.ThrowIfCancellationRequested();
			var reg = new FunctionRegister(Program.Register);
			var options = new Core.Options(Program.Register) {
				Engine = RegEngine.AsRegisteredItem
			};

			//Trace.WriteLine($"{nameof(RunCommand)} 4");
			var func = RegFunction?.Item.Invoke(Program.Register, Layers, options);
			//Trace.WriteLine($"{nameof(RunCommand)} 4.5");
			func.Run(new string[0]); //TODO fix args
									 //Trace.WriteLine($"{nameof(RunCommand)} 5");

			Dispatcher.UIThread.Post(() => {
				//Trace.WriteLine($"{nameof(RunCommand)} 6");
				((ImageStorage.LayersInside)Layers).RefreshAll(); //TODO this still doesn't seem to work..
			});
		}
	}
	//public delegate void ImagesUpdatedHandler(object sender, EventArgs args);
	//public event ImagesUpdatedHandler ImagesUpdated;

	public void CancelCommand()
	{
		var task = SingleTasks.Get(nameof(RunCommand));
		task?.Cancel();
	}

	void RePopulateInputControls(IUsageProvider provider, CancellationToken token)
	{
		InputsList.Clear();

		var usage = provider.GetUsageInfo();
		foreach(var p in usage.Parameters) {
			if (p is IUsageParameter iup) {
				var input = DetermineInputControl(usage, iup);
				if (input != null) { InputsList.Add(input); }
			}
			else {
				//just text so skip
			}
			token.ThrowIfCancellationRequested();
		}
	}

	InputItem DetermineInputControl(Usage usage, IUsageParameter iup)
	{
		//bool isTwo = p is IUsageParameterTwo; //TODO
		var it = iup.InputType.UnWrapNullable();

		if (iup is UsageRegistered ur) {
			IEnumerable<string> names = Program.Register.All(ur.NameSpace)
				.Select(i => i.Name)
			;
			// TODO make a regiered item specific model
			// and change mechanism to use side bar
			// include toggle 'sync' button so we can defaults don't immediately dissapear
			return new InputItemDropDown(iup, names);
		}
		else if (it.IsBool()) {
			return new InputItem(iup);
		}
		else if (it.IsEnum) {
			IUsageEnum iue = null;
			foreach(var i in usage.EnumParameters) {
				if (i.EnumType.Equals(iup.InputType)) {
					iue = i; break;
				}
			}
			return new InputItemDropDown(iup, iue);
		}
		else if (it.IsString()) {
			return new InputItemText(iup);
		}
		else if (it.IsColorRGBA()) {
			//TODO color picker ?
			return null;
		}
		else if (it.IsColor()) {
			//TODO color picker ?
			return null;
		}
		else if (it.IsPoint()) {
			//TODO point picker .. ?
			return null;
		}
		else if (it.IsNumeric()) {
			return new InputItemSlider(iup);
		}

		throw Core.Squeal.NotSupported($"Type {it}");
	}

	public ObservableCollection<InputItem> InputsList { get; init; } = new();

	string _showCommandUsageText = "";
	public string ShowCommandUsageText {
		get => _showCommandUsageText;
		set => this.RaiseAndSetIfChanged(ref _showCommandUsageText, value);
	}

	// public void OnInputsClick(object sender, Avalonia.Interactivity.RoutedEventArgs args)
	// {
	// 	if (sender is not CheckBox box) { return; }
	// 	Log.Debug($"model click {box.Name} {box.IsChecked}");
	// }
}
