using Avalonia;
using Avalonia.Media;
using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Numerics;

namespace ImageFunctions.Gui.ViewModels;

public class InputItem : ViewModelBase
{
	static InputItem()
	{
		IconMultiAdd = AvaloniaTools.GetIconFromName("IconPlusCircle");
		IconMultiRemove = AvaloniaTools.GetIconFromName("IconCloseCircle");
	}

	static readonly StreamGeometry IconMultiAdd;
	static readonly StreamGeometry IconMultiRemove;

	public InputItem(IUsageText input)
	{
		Input = input;
	}

	public IUsageText Input { get; init; }
	public string Description { get { return Input.Description; } }
	public string Name { get { return Input.Name; } }

	// alternate name feature
	public string Alternate { get; init; }

	//multiple of same parameter feature
	public bool MultipleEnabled { get { return AddOrRemoveHandler != null; } }
	public bool IsMultiplePrimary { get { return MultipleIndex == 0; } }
	public Action<InputItem> AddOrRemoveHandler { get; init; }
	public int MultipleIndex { get; init; }

	public string NameDisplay {
		get {
			return String.IsNullOrWhiteSpace(Alternate) ? Name : $"{Name} / {Alternate}";
		}
	}

	public string MultiButtonTag {
		get {
			return IsMultiplePrimary ? "Add another of this parameter" : "Remove this parameter";
		}
	}

	public StreamGeometry MultiIcon {
		get {
			return IsMultiplePrimary ? IconMultiAdd : IconMultiRemove;
		}
	}

	//enabled means the input item has been checked in the ui
	bool _enabled;
	public bool Enabled {
		get => _enabled;
		set => this.RaiseAndSetIfChanged(ref _enabled, value);
	}

	public void AddOrRemoveInputItem()
	{
		//Trace.WriteLine("CloneOrRemoveInputItem");
		AddOrRemoveHandler?.Invoke(this);
	}
}

public class InputItemSlider : InputItem
{
	public InputItemSlider(IUsageParameter input) : base(input)
	{
		//using var deplay = this.DelayChangeNotifications();
		NumberType = input.InputType.UnWrapNullable();
		IsNumberPct = input.IsNumberPct;
		SetDefaultsFromType(input);
		if(IsNumberPct) { ShowAsPct = true; }
	}

	public double Min { get; private set; }
	public double Max { get; private set; }
	public bool IsNumberPct { get; private set; }
	readonly Type NumberType;

	double _value;
	public double Value {
		get { return _value; }
		set {
			//var st = new System.Diagnostics.StackTrace();
			//Log.Debug($"set Value {value} {st.ToString()}");
			string d = FormatValueForDisplay(value);
			this.RaiseAndSetIfChanged(ref _value, value);
			this.RaiseAndSetIfChanged(ref _display, d, nameof(Display));
		}
	}

	string _display;
	public string Display {
		get { return _display; }
		set {
			//Log.Debug($"set Display {value}");
			double v = FormatDisplayForValue(value);
			this.RaiseAndSetIfChanged(ref _display, value);
			this.RaiseAndSetIfChanged(ref _value, v, nameof(Value));
		}
	}

	bool _showAsPct;
	public bool ShowAsPct {
		get { return _showAsPct; }
		set {
			//Log.Debug($"set ShowAsPct {value}");
			this.RaiseAndSetIfChanged(ref _showAsPct, value);
			Display = FormatValueForDisplay(_value);
		}
	}

	string FormatValueForDisplay(double val)
	{
		var trip = RoundTripConvert(val);
		var s = (ShowAsPct ? trip * 100 : trip).ToString();

		//var s = val.ToString();
		//var s = val.ToString("N");
		//Log.Debug($"FormatValueForDisplay {s}");
		return s;
	}

	double FormatDisplayForValue(string display)
	{
		if(!double.TryParse(display, System.Globalization.NumberStyles.Any, null, out var val)) {
			val = _value;
			//TODO error handle ?
		}
		var trip = RoundTripConvert(val);
		return ShowAsPct ? trip / 100.0 : trip;
	}

	double RoundTripConvert(double val)
	{
		var orig = Convert.ChangeType(val, NumberType);
		var trip = Convert.ToDouble(orig);
		return trip;
	}

	void SetDefaultsFromType(IUsageParameter input)
	{
		// https://stackoverflow.com/questions/503263/how-to-determine-if-a-type-implements-a-specific-generic-interface-type
		bool isMinMax = NumberType.GetInterfaces().Any(x =>
			x.IsGenericType &&
			x.GetGenericTypeDefinition() == typeof(IMinMaxValue<>)
		);

		if(isMinMax) {
			double defMin, defMax;
			if(IsNumberPct) {
				defMin = 0.0;
				defMax = 1.0;
			}
			else if(NumberType.Is<double>()) {
				//using the full double min max breaks the slider
				defMin = float.MinValue;
				defMax = float.MaxValue;
			}
			else {
				//Note int.min/max is only used for the name
				defMin = Convert.ToDouble(NumberType.GetField(nameof(int.MinValue)).GetValue(null));
				defMax = Convert.ToDouble(NumberType.GetField(nameof(int.MaxValue)).GetValue(null));
			}
			Min = input.Min ?? defMin;
			Max = input.Max ?? defMax;
			//Log.Debug($"{input.Name} {NumberType.Name} min={Min} max={Max}");
		}
		else {
			throw Squeal.NotSupported($"Type {NumberType.Name}");
		}

		//Log.Debug($"{input?.Name} {input?.Default?.GetType()?.Name} {input?.Default}");
		Value = input.Default == null ? 0.0 : Convert.ToDouble(input.Default);
		//Log.Debug($"{input?.Name} val={Value}");
	}
}

public class InputItemDropDown : InputItem
{
	public InputItemDropDown(IUsageParameter input, IUsageEnum @enum) : base(input)
	{
		var valsList = Rasberry.Cli.PrintHelper.EnumAll(@enum.EnumType, @enum.ExcludeZero);

		int selIndex = 0;
		foreach(var item in valsList) {
			string num = ((int)item).ToString();
			var name = @enum.NameMap != null ? @enum.NameMap(item) : item.ToString();
			var tag = @enum.DescriptionMap != null ? @enum.DescriptionMap(item) : null;

			var sel = new SelectionItem() { Name = $"{num}. {name}", Tag = tag, Value = item };
			Choices.Add(sel);

			if(input.Default != null && input.Default.Equals(item)) {
				SelectedIndex = selIndex;
			}
			selIndex++;
		}
	}

	public InputItemDropDown(IUsageParameter input, IEnumerable<string> @enum) : base(input)
	{
		int selIndex = 0;
		foreach(var name in @enum) {
			var sel = new SelectionItem() { Name = name, Value = name };
			Choices.Add(sel);

			if(input.Default != null && input.Default.Equals(name)) {
				SelectedIndex = selIndex;
			}
			selIndex++;
		}
	}

	public ObservableCollection<SelectionItem> Choices { get; init; } = new();

	int _selectedIndex;
	public int SelectedIndex {
		get => _selectedIndex;
		set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
	}
}

public class InputItemSync : InputItem, IDisposable
{
	static InputItemSync()
	{
		IconSyncData = AvaloniaTools.GetIconFromName("IconSync");
		IconSyncOffData = AvaloniaTools.GetIconFromName("IconSyncOff");
	}

	static readonly StreamGeometry IconSyncData;
	static readonly StreamGeometry IconSyncOffData;

	public InputItemSync(IUsageParameter input, SelectionViewModel svModel) : base(input)
	{
		var reg = Program.Register;
		NameSpace = svModel.NameSpace;

		//when the Registered item selection changes, update Item
		SubSelected = svModel.WhenAnyValue(v => v.Selected)
			.Subscribe(SetItemWhenConnected);
		SubIsSyncEnabled = this.WhenAnyValue(v => v.IsSyncEnabled)
			.Subscribe(s => SetItemWhenConnected(svModel.Selected));

		var defName = reg.Default(NameSpace);
		if(!String.IsNullOrEmpty(defName)) {
			Item = new SelectionItem { Name = defName, NameSpace = NameSpace, Value = defName };
		}
		SetSyncIcon();
	}

	void SetItemWhenConnected(SelectionItem item)
	{
		if(item == null) { return; }
		//Trace.WriteLine($"InputItemSync Item Changed {item.Name}");
		if(this.IsSyncEnabled) {
			this.Item = item;
		}
	}

	public string NameSpace { get; private set; }
	public string Tag { get { return $"Synchronize with {NameSpace}"; } }

	StreamGeometry _syncIcon;
	public StreamGeometry SyncIcon {
		get => _syncIcon;
		set => this.RaiseAndSetIfChanged(ref _syncIcon, value);
	}

	void SetSyncIcon()
	{
		SyncIcon = IsSyncEnabled ? IconSyncData : IconSyncOffData;
	}

	readonly IDisposable SubSelected;
	readonly IDisposable SubIsSyncEnabled;

	public virtual void Dispose()
	{
		SubSelected?.Dispose();
		SubIsSyncEnabled?.Dispose();
		GC.SuppressFinalize(this);
	}

	bool _isSyncEnabled;
	public bool IsSyncEnabled {
		get { return _isSyncEnabled; }
		set {
			this.RaiseAndSetIfChanged(ref _isSyncEnabled, value);
			SetSyncIcon();
		}
	}

	SelectionItem _item;
	public SelectionItem Item {
		get => _item;
		set => this.RaiseAndSetIfChanged(ref _item, value);
	}
}

public class InputItemText : InputItem
{
	public InputItemText(IUsageParameter input) : base(input)
	{
		if(input.Default != null) {
			Text = input.Default.ToString();
		}
	}

	string _text;
	public string Text {
		get => _text ?? "";
		set => this.RaiseAndSetIfChanged(ref _text, value);
	}
}

public class InputItemInfo : InputItem
{
	public InputItemInfo(IUsageText input, IEnumerable<string> lines) : base(input)
	{
		CombinedInfo = String.Join('\n', lines);
	}

	public string CombinedInfo { get; init; }
}

public sealed class InputItemColor : InputItemSync
{
	public InputItemColor(IUsageParameter input, SelectionViewModel model) : base(input, model)
	{
		SubItem = this.WhenAnyValue(v => v.Item).Subscribe(SetColorFromItem);

		if(input.Default != null) {
			if(input.Default is Color native) {
				Color = native;
			}
			else if(input.Default is ColorRGBA rgba) {
				Color = rgba.ToColor();
			}
			else if(input.Default is System.Drawing.Color sdcolor) {
				Color = Color.FromArgb(sdcolor.A, sdcolor.R, sdcolor.G, sdcolor.B);
			}
			else {
				var typeName = input.Default.GetType().FullName;
				throw Squeal.NotSupported($"Color Type {typeName}");
			}
		}
	}

	public static bool IsSupportedColorType(Type it)
	{
		return it.Is<ColorRGBA>() || it.Is<Color>() || it.Is<System.Drawing.Color>();
	}

	void SetColorFromItem(SelectionItem item)
	{
		if(item == null) { return; }
		//Trace.WriteLine($"SetValueFromItem {item.Name} - {item.Value}");
		Color = ((ColorRGBA)item.Value).ToColor();
	}

	readonly IDisposable SubItem;

	public override void Dispose()
	{
		base.Dispose();
		SubItem?.Dispose();
	}

	Color _color;
	public Color Color {
		get => _color;
		set => this.RaiseAndSetIfChanged(ref _color, value);
	}
}

public sealed class InputItemPoint : InputItem, IDisposable
{
	public InputItemPoint(IUsageParameter input, MainWindowViewModel mwvm) : base(input)
	{
		MWVModel = mwvm;
		SubPreviewPointerPos = MWVModel.WhenAnyValue(m => m.PreviewPointerPos)
			.Subscribe(p => PickedPoint = p);
		SubIsPickingFromPreview = MWVModel.WhenAnyValue(m => m.IsPickingFromPreview)
			.Subscribe(SetIsPickingAlone);

		if(input.Default != null) {
			Point point;
			if(input.Default is Point native) {
				point = native;
			}
			else if(input.Default is Core.PointD cpd) {
				point = new Point(cpd.X, cpd.Y);
			}
			else if(input.Default is System.Drawing.Point sdp) {
				point = new Point(sdp.X, sdp.Y);
			}
			else if(input.Default is System.Drawing.PointF sdpf) {
				point = new Point(sdpf.X, sdpf.Y);
			}
			else {
				var typeName = input.Default.GetType().FullName;
				throw Squeal.NotSupported($"Point Type {typeName}");
			}

			PickedPoint = point;
		}

		if(input.InputType != null) {
			var it = input.InputType;
			if(it.Is<System.Drawing.PointF>()) {
				CoordType = typeof(float);
			}
			else if(it.Is<System.Drawing.Point>()) {
				CoordType = typeof(int);
			}
			//covers Avalonia.Point and Core.PointD
			else {
				CoordType = typeof(double);
			}
		}
	}

	readonly Type CoordType;
	readonly MainWindowViewModel MWVModel;

	bool _isPicking;
	public bool IsPicking {
		get {
			return _isPicking;
		}
		set {
			MWVModel.IsPickingFromPreview = value;
			SetIsPickingAlone(value);
		}
	}

	//used to avoid an infinite event loop with IsPickingFromPreview
	void SetIsPickingAlone(bool val)
	{
		if(val != _isPicking) {
			this.RaisePropertyChanging(nameof(IsPicking));
			_isPicking = val;
			this.RaisePropertyChanged(nameof(IsPicking));
		}
	}

	string FormatCoord(double raw)
	{
		if(CoordType.Is<int>()) {
			//spacial case-ing because Convert.ChangeType is rounding the double instead of flooring 🤷
			return ((int)raw).ToString();
		}
		else {
			return Convert.ChangeType(raw, CoordType).ToString();
		}
	}

	public string PickedX {
		get {
			return FormatCoord(PickedPoint.X);
		}
		set {
			var newVal = Convert.ToDouble(value);
			if(PickedPoint.X != newVal) {
				PickedPoint = new Point(newVal, PickedPoint.Y);
			}
		}
	}

	public string PickedY {
		get {
			return FormatCoord(PickedPoint.Y);
		}
		set {
			var newVal = Convert.ToDouble(value);
			if(PickedPoint.Y != newVal) {
				PickedPoint = new Point(PickedPoint.X, newVal);
			}
		}
	}

	Point _pickedPoint;
	public Point PickedPoint {
		get {
			return _pickedPoint;
		}
		set {
			if(_pickedPoint != value) {
				this.RaisePropertyChanging(nameof(PickedX));
				this.RaisePropertyChanging(nameof(PickedY));
				this.RaisePropertyChanging(nameof(PickedPoint));
				_pickedPoint = value;
				this.RaisePropertyChanged(nameof(PickedX));
				this.RaisePropertyChanged(nameof(PickedY));
				this.RaisePropertyChanged(nameof(PickedPoint));
			}
		}
	}

	public static bool IsSupportedPointType(Type it)
	{
		return it.Is<Point>()
			|| it.Is<PointD>()
			|| it.Is<System.Drawing.Point>()
			|| it.Is<System.Drawing.PointF>()
		;
	}

	readonly IDisposable SubPreviewPointerPos;
	readonly IDisposable SubIsPickingFromPreview;

	public void Dispose()
	{
		SubPreviewPointerPos?.Dispose();
		SubIsPickingFromPreview?.Dispose();
	}
}
