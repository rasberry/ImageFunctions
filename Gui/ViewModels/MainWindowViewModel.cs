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
		InputsList.WatchChildProperties(OnInputListPropChanged, OnInputListChanged);

		// this.WhenAnyValue(v => v.CommandText)
		// 	.Subscribe(UpdateWidgetsFromCommandLine);
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

			var opts = func.Core;
			var sb = new StringBuilder();
			func?.Core.Usage(sb, Program.Register);
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

	public void UpdatePreviewZoomByScroll(Vector delta)
	{
		//CurrentZoom.ViewPort = viewPort;
		if(delta.Y > 0) {
			CurrentZoom.Bigger();
		}
		else if(delta.Y < 0) {
			CurrentZoom.Smaller();
		}
	}

	public ZoomViewModel CurrentZoom { get; private set; } = new();

	public ReadOnlyCollection<ZoomHelperDisplayItem> ZoomOptions {
		get {
			return ZoomViewModel.Items;
		}
	}

	public void PreviewZoomIn()
	{
		CurrentZoom.Bigger();
	}
	public void PreviewZoomOut()
	{
		CurrentZoom.Smaller();
	}
	public void PreviewZoomReset()
	{
		CurrentZoom.Reset();
	}

	//global flag when a InputItemPoint control is picking
	bool _isPickingFromPreview;
	public bool IsPickingFromPreview {
		get => _isPickingFromPreview;
		set => this.RaiseAndSetIfChanged(ref _isPickingFromPreview, value);
	}

	//global position of pointer when InputItemPoint control is picking
	Point _previewPointerPos;
	public Point PreviewPointerPos {
		get => _previewPointerPos;
		set => this.RaiseAndSetIfChanged(ref _previewPointerPos, value);
	}

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

	public void LoadAndShowImage(string fileName)
	{
		if(!CheckIsEngineSelected()) { return; }
		// Trace.WriteLine($"{nameof(LoadAndShowImage)} {fileName}");
		using var clerk = new FileClerk(FileIO, fileName);
		RegEngine.LoadImage(Layers, clerk);
	}

	public void SaveImage(string fileName, string format, bool doSaveStack)
	{
		if(!CheckIsEngineSelected()) { return; }
		var layers = MakeUnwrappedLayers(!doSaveStack);
		using var clerk = new FileClerk(FileIO, fileName);
		RegEngine.SaveImage(layers, clerk, format);
	}

	Layers MakeUnwrappedLayers(bool topOnly)
	{
		var layers = new Layers(); //don't dispose since we're just referencing
		if(topOnly) {
			var first = Layers.First();
			layers.Push(tryUnwrap(first.Canvas), first.Name);
		}
		else {
			foreach(var l in Layers) {
				//push to the end since we want the order to not get reversed
				layers.PushAt(layers.Count, tryUnwrap(l.Canvas), l.Name);
			}
		}

		return layers;

		static ICanvas tryUnwrap(ICanvas canvas)
		{
			if(canvas is CanvasWrapper wrap) {
				return wrap.Unwrap();
			}
			return canvas;
		}
	}

	bool CheckIsEngineSelected()
	{
		if(RegEngine == null) {
			var txt = GuiNote.WarningMustBeSelected("engine");
			UpdateStatusText(txt, WarningTimeout, LogCategory.Warning);
			return false;
		}
		return true;
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

	public void CancelCommand(object sender, EventArgs args)
	{
		var task = SingleTasks.Get(nameof(RunCommand));
		task?.Cancel();
		OverlayState.IsPopupVisible = false;
	}

	void RePopulateInputControls(IUsageProvider provider, CancellationToken token)
	{
		CurrentUsage = provider.GetUsageInfo();
		InputsList.RemoveDisposeAll();
		CommandLineArgCache.Clear();
		MultipleParameterCount.Clear();
		// Trace.WriteLine($"RePopulateInputControls CLAC={CommandLineArgCache.Count} IL={InputsList.Count}");

		var ud = CurrentUsage.Description;
		if((ud?.Descriptions?.Any()).GetValueOrDefault(false)) {
			var iii = new InputItemInfo(
				new UsageText(1, "", ""),
				CurrentUsage.Description.Descriptions
			);
			InputsList.Add(iii);
		}

		foreach(var p in CurrentUsage.Parameters) {
			if(p is IUsageParameter iup) {
				var input = DetermineInputControl(iup);
				if(input != null) {
					InputsList.Add(input);
				}
			}
			else {
				//just text so skip
			}
			token.ThrowIfCancellationRequested();
		}
	}

	readonly Dictionary<string, int> MultipleParameterCount = new();
	int MultipleParameterCounter = 0; //used to make sure each parameter has it's own id
	Usage CurrentUsage;

	void MakeOrRemoveInputItem(InputItem item)
	{
		if(!item.MultipleEnabled) { return; }
		string name = item.Name;
		if(!MultipleParameterCount.TryGetValue(name, out int count)) {
			MultipleParameterCount.Add(name, 1);
			count = 1;
		}

		var index = InputsList.IndexOf(item);
		if(item.IsMultiplePrimary) { //adding
			var itemMany = (IUsageMany)item.Input;
			int max = itemMany.AllowCount;
			if(count >= max) {
				this.UpdateStatusText($"Maximum item count of {max} reached for {item.Name}", null, LogCategory.Info);
				return;
			}
			int ter = Interlocked.Increment(ref MultipleParameterCounter);
			//Trace.WriteLine($"Add ix={index} mi={item.MultipleIndex} c={count} nix={index + count}");
			var input = DetermineInputControl(itemMany, index + ter);
			if(input != null) {
				MultipleParameterCount[name]++;
				InputsList.Insert(index + count, input);
			}
		}
		else { //removing
			   //Trace.WriteLine($"Remove ix={index} mi={item.MultipleIndex} c={count}");
			MultipleParameterCount[name]--;
			InputsList.RemoveDisposeAt(index);
		}
	}

	InputItem DetermineInputControl(IUsageParameter iup, int multiIndex = 0)
	{
		var it = iup.InputType.UnWrapNullable();
		var altSet = CurrentUsage.Alternates?.ToDictionary(k => k.Name) ?? null;
		var isMany = iup is IUsageMany ium;

		//helper alt lookup function to avoid a bunch of repeat code
		string GetAlt(string name)
		{
			if(altSet != null && altSet.TryGetValue(name, out var alt)) {
				return alt.Alternate;
			}
			return null;
		}

		InputItem final = null;
		if(iup is UsageRegistered ur) {
			//not implementing multiple for register items since i can't think of a use-case
			var model = RegisteredControlList.First(svm => svm.NameSpace == ur.NameSpace);
			System.Diagnostics.Trace.WriteLine($"UsageRegistered m:{model == null} n:{model?.NameSpace}");
			final = new InputItemSync(iup, model) { Alternate = GetAlt(iup.Name) };
		}
		else if(it.Is<bool>()) {
			//doesn't make sense for bool types to have multiple
			final = new InputItem(iup) { Alternate = GetAlt(iup.Name) };
		}
		else if(it.IsEnum) {
			IUsageEnum iue = null;
			foreach(var i in CurrentUsage.EnumParameters) {
				if(i.EnumType.Equals(iup.InputType)) {
					iue = i; break;
				}
			}
			final = new InputItemDropDown(iup, iue) {
				Alternate = GetAlt(iup.Name),
				AddOrRemoveHandler = isMany ? MakeOrRemoveInputItem : null,
				MultipleIndex = multiIndex
			};
		}
		else if(it.Is<string>()) {
			final = new InputItemText(iup) {
				Alternate = GetAlt(iup.Name),
				AddOrRemoveHandler = isMany ? MakeOrRemoveInputItem : null,
				MultipleIndex = multiIndex
			};
		}
		//Color inputs also have a sync component
		else if(InputItemColor.IsSupportedColorType(it)) {
			var model = RegisteredControlList.First(svm => svm.NameSpace == "Color");
			final = new InputItemColor(iup, model) {
				Alternate = GetAlt(iup.Name),
				AddOrRemoveHandler = isMany ? MakeOrRemoveInputItem : null,
				MultipleIndex = multiIndex
			};
		}
		else if(InputItemPoint.IsSupportedPointType(it)) {
			final = new InputItemPoint(iup, this) {
				Alternate = GetAlt(iup.Name),
				AddOrRemoveHandler = isMany ? MakeOrRemoveInputItem : null,
				MultipleIndex = multiIndex
			};
		}
		else if(it.IsNumeric()) {
			final = new InputItemSlider(iup) {
				Alternate = GetAlt(iup.Name),
				AddOrRemoveHandler = isMany ? MakeOrRemoveInputItem : null,
				MultipleIndex = multiIndex
			};
		}

		if(final != null) {
			// WeakTrackItem(final);
			return final;
		}
		else {
			throw Squeal.NotSupported($"Type {it}");
		}
	}

	//Keep - test for subscription leaks
	//readonly object TrackerLock = new();
	//readonly List<WeakReference> TrackerList = new();
	// void WeakTrackItem(InputItem item)
	// {
	// 	int wasLen = 0;
	// 	var dist = new Dictionary<string,int>();
	// 	lock(TrackerLock) {
	// 		//add item
	// 		TrackerList.Add(new WeakReference(item));
	// 		//remove dead items
	// 		int len = wasLen = TrackerList.Count;
	// 		for(int t=0; t < len; t++) {
	// 			var wr = TrackerList[t];
	// 			if (wr == null || !wr.IsAlive) {
	// 				//swap out end and remove
	// 				len--;
	// 				TrackerList[t] = TrackerList[len];
	// 				TrackerList.RemoveAt(len);
	// 			}
	// 			var n = wr?.Target?.GetType()?.FullName;
	// 			if (!String.IsNullOrWhiteSpace(n)) {
	// 				if (!dist.ContainsKey(n)) {
	// 					dist[n] = 1;
	// 				}
	// 				else {
	// 					dist[n]++;
	// 				}
	// 			}
	// 		}
	// 	}
	// 	Trace.WriteLine($"WeakTrackItem count={wasLen}");
	// 	foreach(var kvp in dist) {
	// 		Trace.WriteLine($"{kvp.Key} = {kvp.Value}");
	// 	}
	// }

	public ObservableCollection<InputItem> InputsList { get; init; } = new();

	string _showCommandUsageText = "";
	public string ShowCommandUsageText {
		get => _showCommandUsageText;
		set => this.RaiseAndSetIfChanged(ref _showCommandUsageText, value);
	}

	public void OnInputListPropChanged(object sender, PropertyChangedEventArgs args)
	{
		string value = "";
		if(sender is InputItemPoint iipoint) {
			value = $"{iipoint.PickedX},{iipoint.PickedY}";
		}
		if(sender is InputItemColor iicolor) {
			var c = iicolor.Color;
			value = $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
		}
		else if(sender is InputItemSync iisync) {
			var sel = iisync.Item;
			value = sel.Name;
		}
		else if(sender is InputItemSlider iislider) {
			value = iislider.Display + (iislider.ShowAsPct ? "%" : "");
		}
		else if(sender is InputItemText iitext) {
			value = iitext.Text;
		}
		else if(sender is InputItemDropDown iidrop) {
			var sel = iidrop.SelectedIndex >= 0 ? iidrop.Choices[iidrop.SelectedIndex] : null;
			value = sel?.Value.ToString();
		}

		if(sender is InputItem ii) {
			var key = $"{ii.Name}{ii.MultipleIndex}";
			// Trace.WriteLine($"OnInputListChanged {(ii.Enabled?"add":"rem")} {sender.GetType().FullName}: [{ii.MultipleIndex}] {ii.Name}={value}");
			if(ii.Enabled) {
				CommandLineArgCache[key] = (ii.Name, value);
			}
			else {
				CommandLineArgCache.Remove(key);
			}
		}
		else {
			Program.Log.Debug($"?? {sender.GetType().FullName}");
		}

		RenderCommandLineFromWidgets();
	}

	void OnInputListChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if(args.Action == NotifyCollectionChangedAction.Remove) {
			foreach(var item in args.OldItems) {
				if(item is InputItem ii) {
					var key = $"{ii.Name}{ii.MultipleIndex}";
					CommandLineArgCache.Remove(key);
				}
			}
			RenderCommandLineFromWidgets();
		}
	}

	readonly Dictionary<string, (string, string)> CommandLineArgCache = new();

	bool commandLineIsRendering = false;
	void RenderCommandLineFromWidgets()
	{
		if(commandLineIsRendering) { return; }
		commandLineIsRendering = true;

		bool isFirst = true;
		StringBuilder sb = new();
		foreach(var kvp in CommandLineArgCache) {
			var (name, value) = kvp.Value;
			//Trace.WriteLine($"RCLFW [{kvp.Key},{kvp.Value}]");
			sb.Append($"{(isFirst ? "" : " ")}{name} {value}");
			isFirst = false;
		}

		CommandText = sb.ToString();
		commandLineIsRendering = false;
	}

	// 	//TODO maybe get rid of this.. seems complicated
	// void UpdateWidgetsFromCommandLine(string text)
	// {
	// 	if(String.IsNullOrWhiteSpace(text)) { return; }
	// 	if(commandLineIsRendering) { return; }
	// 	commandLineIsRendering = true;

	// 	//var parts = text.Split([' '],StringSplitOptions.RemoveEmptyEntries);

	// 	//Log.Debug($"UpdateWidgetsFromCommandLine {text}");
	// 	commandLineIsRendering = false;
	//	//TODO how to update controls ?
	// }
}
