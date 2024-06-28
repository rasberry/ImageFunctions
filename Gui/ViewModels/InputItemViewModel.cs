using ImageFunctions.Core;
using ImageFunctions.Gui.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Numerics;

namespace ImageFunctions.Gui.ViewModels;

public class InputItem : ViewModelBase
{
	public InputItem(IUsageText input)
	{
		Input = input;
	}
	public IUsageText Input { get; init; }

	public string Name { get { return Input.Name; }}
	public string Description { get { return Input.Description; }}

	bool _enabled;
	public bool Enabled {
		get => _enabled;
		set => this.RaiseAndSetIfChanged(ref _enabled, value);
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
		if (IsNumberPct) { ShowAsPct = true; }
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
		if (!double.TryParse(display,System.Globalization.NumberStyles.Any, null, out var val)) {
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

		if (isMinMax) {
			double defMin, defMax;
			if (IsNumberPct) {
				defMin = 0.0;
				defMax = 1.0;
			}
			else if (NumberType.IsDouble()) {
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

		Log.Debug($"{input?.Name} {input?.Default?.GetType()?.Name} {input?.Default}");
		Value = input.Default == null ? 0.0 : Convert.ToDouble(input.Default);
		Log.Debug($"{input?.Name} val={Value}");
	}
}

public class InputItemDropDown : InputItem
{
	public InputItemDropDown(IUsageParameter input, IUsageEnum @enum) : base(input)
	{
		var valsList = Rasberry.Cli.PrintHelper.EnumAll(@enum.EnumType, @enum.ExcludeZero);

		int index = 0;
		foreach(var item in valsList) {
			string num = ((int)item).ToString();
			var name = @enum.NameMap != null ? @enum.NameMap(item) : item.ToString();
			var	tag = @enum.DescriptionMap != null ? @enum.DescriptionMap(item) : null;

			var sel = new SelectionItem() { Name = $"{num}. {name}", Tag = tag };
			Choices.Add(sel);

			if (input.Default != null && input.Default.Equals(item)) {
				SelectedIndex = index;
			}
			index++;
		}
	}

	public InputItemDropDown(IUsageParameter input, IEnumerable<string> @enum) : base(input)
	{
		int index = 0;
		foreach(var name in @enum) {
			var sel = new SelectionItem() { Name = name };
			Choices.Add(sel);

			if (input.Default != null && input.Default.Equals(name)) {
				SelectedIndex = index;
			}
			index++;
		}
	}

	public ObservableCollection<SelectionItem> Choices { get; init; } = new();

	int _selectedIndex;
	public int SelectedIndex {
		get => _selectedIndex;
		set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
	}
}

public class InputItemText : InputItem
{
	public InputItemText(IUsageParameter input) : base(input)
	{
		if (input.Default != null) {
			Text = input.Default.ToString();
		}
	}

	string _text;
	public string Text {
		get => _text ?? "";
		set => this.RaiseAndSetIfChanged(ref _text, value);
	}
}
