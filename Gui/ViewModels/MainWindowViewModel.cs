using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.FileIO;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.Models;
using ImageFunctions.Plugin.Aides;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
		OverlayState = new();
		OverlayState.OnStopJob += CancelCommand;
		OptionsModel = new();

		RxApp.MainThreadScheduler.Schedule(LoadData);
		LayersImageList.CollectionChanged += OnLayersCollectionChange;

		//Don't know how to 'subscribe' to all child prop changes so just using a wrapper
		InputsList.WatchChildProperties(OnInputListChanged);

		this.WhenAnyValue(v => v.CommandText)
			.Subscribe(UpdateWidgetsFromCommandLine);
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
	}

	SelectionViewModel GetSelectionViewModelForNameSpace(string ns)
	{
		var svm = ns switch {

			FunctionRegister.NS => AddTreeNodeFromRegistered(ns, (reg, item) => {
				return new SelectionItem { Name = item.Name, NameSpace = ns };
			}, OnFunctionSelected),

			// using Value here since ColorRGBA is light-weight
			ColorRegister.NS => AddTreeNodeFromRegistered(ns, (reg, item) => {
				var colorItem = reg.Get<ColorRGBA>(ns, item.Name);
				return new SelectionItemColor {
					Name = item.Name, NameSpace = ns, Value = colorItem.Item
				};
			}),

			EngineRegister.NS => AddTreeNodeFromRegistered(ns, (reg, item) => {
				string tag = reg.GetNameSpaceItemHelp(item);
				return new SelectionItem { Name = item.Name, NameSpace = ns, Tag = tag };
			}, OnEngineSelected),

			_ => AddTreeNodeFromRegistered(ns, (reg, item) => {
				string tag = reg.GetNameSpaceItemHelp(item);
				return new SelectionItem { Name = item.Name, NameSpace = ns, Tag = tag };
			}),
		};

		return svm;
	}

	public OverlayViewModel OverlayState { get; init; }
	public ILayers Layers { get; init; }
	public ObservableStackList<LayersImageData> LayersImageList { get; init; }
	public OptionsViewModel OptionsModel { get; init; }

	SelectionViewModel AddTreeNodeFromRegistered(string @namespace,
		Func<IRegister, INameSpaceName, SelectionItem> filler,
		Action<SelectionItem> selectionHandler = null
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
			if(def != null && item.Name.EqualsIC(def)) {
				selected = item;
			}
		}
		var sel = new SelectionViewModel {
			NameSpace = @namespace,
			Items = items
		};
		if(selected != null) {
			sel.Selected = selected;
		}

		if(selectionHandler != null) {
			sel.WhenAnyValue(p => p.Selected)
				.Subscribe(selectionHandler);
		}

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

		var timeout = TimeSpan.FromMinutes(5);
		var task = SingleTasks.GetOrMake(nameof(OnFunctionSelected), job, timeout);
		_ = task?.Run(); //fire and forget

		void job(CancellationToken token)
		{
			//usage only needs Register and Log
			var ctx = new FunctionContext { Register = Program.Register, Log = Program.Log, Token = token };
			var func = RegFunction?.Item.Invoke(ctx);
			token.ThrowIfCancellationRequested();

			var opts = func.Options;
			var sb = new StringBuilder();
			func?.Options.Usage(sb, Program.Register);
			token.ThrowIfCancellationRequested();

			Dispatcher.UIThread.Post(() => {
				CommandText = "";
				UsageText = sb.ToString();
				if(opts is IUsageProvider iup) {
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

	// void OnSomethingSelected(SelectionItem item)
	// {
	// 	Program.Log.Debug($"Something selected {item?.Name}");
	// }

	public FilePickerFileType SupportedReadTypes { get; private set; }
	public FilePickerFileType SupportedWriteTypes { get; private set; }

	string _statusClass;
	public string StatusClass {
		get => _statusClass;
		set => this.RaiseAndSetIfChanged(ref _statusClass, value);
	}

	public ObservableCollection<StatusHistoryLine> StatusHistory { get; init; } = new();
	public InlineCollection StatusTextInlines { get; init; } = new();

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

	bool _isStatusHistoryOpen = false;
	public bool IsStatusHistoryOpen {
		get => _isStatusHistoryOpen;
		set => this.RaiseAndSetIfChanged(ref _isStatusHistoryOpen, value);
	}

	public void ToggleStatusHistory()
	{
		IsStatusHistoryOpen = !IsStatusHistoryOpen;
	}

	// The behavior shows the text as long as the control is still under the pointer
	// but waits before hiding the text after the pointer leaves
	public void UpdateStatusText(string text, TimeSpan? timeout = null,
		LogCategory category = LogCategory.Unknown)
	{
		//Trace.WriteLine($"UpdateStatusText T:'{text}' E:{(startTimer?"Y":"N")} T:{timeout.GetValueOrDefault().TotalMilliseconds}");
		if(StatusTextTimer == null) {
			StatusTextTimer = new() { AutoReset = false };

			//this clears the status after some time
			StatusTextTimer.Elapsed += (s, e) => {
				//Trace.WriteLine($"{nameof(UpdateStatusText)} Time Stop");
				StatusTextTimer.Stop();
				Dispatcher.UIThread.Invoke(() => {
					StatusTextInlines.Clear(); //clear text
				});
			};
		}

		DrawStatusText(text, category);
		if(category != LogCategory.Unknown) {
			AddStatusToHistory(text, category);
		}
		StatusTextTimer.Interval = timeout != null ? timeout.Value.TotalMilliseconds : StatusTextTimeout.TotalMilliseconds;
		StatusTextTimer.Start();
		//Trace.WriteLine($"{nameof(UpdateStatusText)} Time Start {StatusTextTimer.Interval}");
	}

	//Elapsed method needs access to instance members so can't static initialize
	System.Timers.Timer StatusTextTimer = null;

	void DrawStatusText(string text, LogCategory category)
	{
		StatusClass = StatusHistoryLine.GetClassForCategory(category);
		StatusTextInlines.Clear();
		if(!String.IsNullOrWhiteSpace(text)) {
			StatusHistoryLine.CreateStatusRun(StatusTextInlines, text, category);
		}
		//scroll to the bottom to show latest history
		StatusHistoryScrollOffset = new Vector(0.0, double.PositiveInfinity);
	}

	const int MaxStatusHistorySize = 50;
	void AddStatusToHistory(string text, LogCategory category)
	{
		//this is drawn top to bottom but we want the items to drop-off the top
		//so adding new items to the end (bottom) and removing them from the beginning (top)
		StatusHistory.Add(new StatusHistoryLine(text, category));
		if(StatusHistory.Count > MaxStatusHistorySize) {
			StatusHistory.RemoveAt(0);
		}
	}

	Vector _statusHistoryScrollOffset;
	public Vector StatusHistoryScrollOffset {
		get => _statusHistoryScrollOffset;
		set => this.RaiseAndSetIfChanged(ref _statusHistoryScrollOffset, value);
	}

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

	public void LayersNewTop()
	{
		var w = OptionsModel.InitialLayerWidth;
		var h = OptionsModel.InitialLayerHeight;
		var canvas = RegEngine.NewCanvasFromLayersOrDefault(Layers, w, h);
		Layers.Push(canvas);
	}

	public void LayersCloneTop()
	{
		if(Layers.Count < 1) {
			var txt = Note.NoLayersPresent();
			UpdateStatusText(txt, WarningTimeout, LogCategory.Warning);
		}
		else {
			var orig = Layers[0].Canvas;
			var copy = RegEngine.NewCanvas(orig.Width, orig.Height);
			copy.CopyFrom(orig);
			Layers.Push(copy);
		}
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
			UpdateStatusText(txt, WarningTimeout, LogCategory.Warning);
			return;
		}

		Trace.WriteLine($"{nameof(LoadAndShowImage)} {fileName}");
		using var clerk = new FileClerk(FileIO, fileName);
		RegEngine.LoadImage(Layers, clerk);
	}

	readonly SimpleFileIO FileIO = new();

	WriteableBitmap ConvertCanvasToRgba8888(ICanvas canvas)
	{
		//var previewBounds = RectSizeToPixels(PreviewRectangle, StandardDpi); //TODO this is definitely wrong
		var workBounds = new Rect(0, 0, canvas.Width, canvas.Height);
		//var workBounds = bounds == null ? imgBounds : imgBounds.Intersect(bounds);

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

		//TODO not sure if serial or parallel loop is better
		//Trace.WriteLine($"Rgba8888 {wWidth} {wHeight} {wTop} {wBottom} {wLeft} {wRight}");
		// int dataOffset = 0;
		// for(int y = wTop; y < wBottom; y++) {
		// 	for(int x = wLeft; x < wRight; x++) {
		// 		var pix = canvas[x, y];
		// 		data[dataOffset + 0] = (byte)(pix.R * 255.0);
		// 		data[dataOffset + 1] = (byte)(pix.G * 255.0);
		// 		data[dataOffset + 2] = (byte)(pix.B * 255.0);
		// 		data[dataOffset + 3] = (byte)(pix.A * 255.0);
		// 		dataOffset += BytesPerPixel;
		// 	}
		// }

		Parallel.For(0, wWidth * wHeight, (index) => {
			int dataOffset = index * BytesPerPixel;
			int x = index % wWidth;
			int y = index / wWidth;
			var pix = canvas[x, y];
			data[dataOffset + 0] = (byte)(pix.R * 255.0);
			data[dataOffset + 1] = (byte)(pix.G * 255.0);
			data[dataOffset + 2] = (byte)(pix.B * 255.0);
			data[dataOffset + 3] = (byte)(pix.A * 255.0);
		});

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
		//Trace.WriteLine(nameof(RunCommand));
		if(RegFunction == null) {
			var txt = GuiNote.WarningMustBeSelected("function");
			UpdateStatusText(txt, WarningTimeout, LogCategory.Warning);
			return;
		}
		if(RegEngine == null) {
			var txt = GuiNote.WarningMustBeSelected("engine");
			UpdateStatusText(txt, WarningTimeout, LogCategory.Warning);
			return;
		}

		if(OverlayDelayTimer == null) {
			OverlayDelayTimer = new();
			OverlayDelayTimer.Elapsed += (s, e) => {
				Dispatcher.UIThread.Post(() => {
					OverlayDelayTimer.Stop();
					OverlayState.Label = $"Running {RegFunction?.Name}";
					OverlayState.IsPopupVisible = true;
				});
			};
		}

		//Trace.WriteLine($"{nameof(RunCommand)} 2");
		var task = SingleTasks.GetOrMake(nameof(RunCommand), job, CommandTimeout);
		_ = task.Run();

		OverlayDelayTimer.Interval = OverlayDelayTimout.TotalMilliseconds;
		OverlayDelayTimer.Start();

		void job(CancellationToken token)
		{
			var progress = new ProgressTracker();
			progress.OnReport += (s, e) => {
				double amount = Math.Clamp(e.Amount, 0.0, 1.0);
				OverlayState.ProgressAmount = amount;
			};

			//Trace.WriteLine($"{nameof(RunCommand)} 3");
			token.ThrowIfCancellationRequested();
			//var reg = new FunctionRegister(Program.Register);
			var logger = new GuiLogger();
			logger.OnLogEvent += (s, e) => {
				Dispatcher.UIThread.Post(() => {
					UpdateStatusText(e.Message, WarningTimeout, e.Category);
				});
			};

			var context = new FunctionContext {
				Register = Program.Register,
				Log = logger,
				Options = new BasicOptions {
					Register = Program.Register,
					Engine = RegEngine.AsRegisteredItem
				},
				Layers = Layers,
				Progress = progress,
				Token = token
			};

			//Trace.WriteLine($"{nameof(RunCommand)} 4");
			var func = RegFunction?.Item.Invoke(context);
			//Trace.WriteLine($"{nameof(RunCommand)} 4.5 {RegFunction?.Name}");
			var args = AvaloniaTools.SplitCommandLine(this.CommandText).ToArray();
			//Trace.WriteLine($"{nameof(RunCommand)} '{this.CommandText}' '{String.Join(' ',args)}'");
			func.Run(args);
			//Trace.WriteLine($"{nameof(RunCommand)} 5");

			Dispatcher.UIThread.Post(() => {
				OverlayDelayTimer.Stop();
				OverlayState.IsPopupVisible = false;
				//Trace.WriteLine($"{nameof(RunCommand)} 6");
				((ImageStorage.LayersInside)Layers).RefreshAll();
			});
		}
	}
	static readonly TimeSpan OverlayDelayTimout = TimeSpan.FromMilliseconds(200);
	System.Timers.Timer OverlayDelayTimer;

	//public delegate void ImagesUpdatedHandler(object sender, EventArgs args);
	//public event ImagesUpdatedHandler ImagesUpdated;

	public void CancelCommand(object sender, EventArgs args)
	{
		var task = SingleTasks.Get(nameof(RunCommand));
		task?.Cancel();
		OverlayState.IsPopupVisible = false;
	}

	void RePopulateInputControls(IUsageProvider provider, CancellationToken token)
	{
		InputsList.Clear();
		var usage = provider.GetUsageInfo();

		var ud = usage.Description;
		if((ud?.Descriptions?.Any()).GetValueOrDefault(false)) {
			var iii = new InputItemInfo(new UsageText(1, "", ""), usage.Description.Descriptions);
			InputsList.Add(iii);
		}

		foreach(var p in usage.Parameters) {
			if(p is IUsageParameter iup) {
				var input = DetermineInputControl(usage, iup);
				if(input != null) { InputsList.Add(input); }
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

		if(iup is UsageRegistered ur) {
			var model = RegisteredControlList.First(svm => svm.NameSpace == ur.NameSpace);
			return new InputItemSync(iup, model);
		}
		else if(it.Is<bool>()) {
			return new InputItem(iup);
		}
		else if(it.IsEnum) {
			IUsageEnum iue = null;
			foreach(var i in usage.EnumParameters) {
				if(i.EnumType.Equals(iup.InputType)) {
					iue = i; break;
				}
			}
			return new InputItemDropDown(iup, iue);
		}
		else if(it.Is<string>()) {
			return new InputItemText(iup);
		}
		//Color inputs also have a sync component
		else if(it.Is<ColorRGBA>() || it.Is<System.Drawing.Color>()) {
			var model = RegisteredControlList.First(svm => svm.NameSpace == "Color");
			return new InputItemColor(iup, model);
		}
		else if(it.Is<System.Drawing.Point>() || it.Is<System.Drawing.PointF>() || it.Is<PointD>()) {
			//TODO point picker .. ?
			return null;
		}
		else if(it.IsNumeric()) {
			return new InputItemSlider(iup);
		}

		throw Squeal.NotSupported($"Type {it}");
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

	public void OnInputListChanged(object sender, PropertyChangedEventArgs args)
	{
		//string extra = "";
		string value = "";
		if(sender is InputItemColor iicolor) {
			//Trace.WriteLine($"OnInputListChanged InputItemColor {(iicolor == null ? "null" : "good")}");
			var c = iicolor.Color;
			value = $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
		}
		else if(sender is InputItemSync iisync) {
			var sel = iisync.Item;
			//Trace.WriteLine($"InputItemSync IsSyncEnabled={iisync.IsSyncEnabled} INS={sel?.NameSpace} IN={sel?.Name} V={sel?.Value}");
			value = sel.Name;
		}
		else if(sender is InputItemSlider iislider) {
			//extra = $"InputItemSlider Value={iislider.Value}";
			value = iislider.Display + (iislider.ShowAsPct ? "%" : "");
		}
		else if(sender is InputItemText iitext) {
			//extra = $"InputItemInfo Text={iitext.Text}";
			value = iitext.Text;
		}
		else if(sender is InputItemDropDown iidrop) {
			var sel = iidrop.SelectedIndex >= 0 ? iidrop.Choices[iidrop.SelectedIndex] : null;
			//extra = $"InputItemDropDown SelectedIndex={iidrop.SelectedIndex} INS={sel?.NameSpace} IN={sel?.Name} V={sel?.Value}";
			value = sel?.Value.ToString();
		}

		if(sender is InputItem ii) {
			//Log.Debug($"{(ii.Enabled?"✔":"❌")} [{ii.Name}] {extra}");
			if(ii.Enabled) {
				CommandLineArgCache[ii.Name] = value;
			}
			else {
				CommandLineArgCache.Remove(ii.Name);
			}
		}
		else {
			Program.Log.Debug($"?? {sender.GetType().FullName}");
		}

		RenderCommandLineFromWidgets();
	}

	readonly Dictionary<string, string> CommandLineArgCache = new();

	bool commandLineIsRendering = false;
	void RenderCommandLineFromWidgets()
	{
		if(commandLineIsRendering) { return; }
		commandLineIsRendering = true;

		bool isFirst = true;
		StringBuilder sb = new();
		foreach(var kvp in CommandLineArgCache) {
			sb.Append($"{(isFirst ? "" : " ")}{kvp.Key} {kvp.Value}");
			isFirst = false;
		}

		CommandText = sb.ToString();
		commandLineIsRendering = false;
	}

	void UpdateWidgetsFromCommandLine(string text)
	{
		if(String.IsNullOrWhiteSpace(text)) { return; }
		if(commandLineIsRendering) { return; }
		commandLineIsRendering = true;

		//TODO maybe get rid of this.. seems complicated
		//var parts = text.Split([' '],StringSplitOptions.RemoveEmptyEntries);

		//Log.Debug($"UpdateWidgetsFromCommandLine {text}");
		commandLineIsRendering = false;
	}
}
