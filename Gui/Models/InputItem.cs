using ImageFunctions.Core;
using ImageFunctions.Gui.ViewModels;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Numerics;

namespace ImageFunctions.Gui.Models;

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
		SetDefaultsFromType(input);
	}

	public double Min { get; private set; }
	public double Max { get; private set; }

	double _value;
	public double Value {
		get { return _value; }
		set {
			string d = FormatValueForDisplay(value);
			this.RaiseAndSetIfChanged(ref _value, value);
			this.RaiseAndSetIfChanged(ref _display, d, nameof(Display));
		}
	}

	string _display;
	public string Display {
		get { return _display; }
		set {
			double v = FormatDisplayForValue(value);
			this.RaiseAndSetIfChanged(ref _display, value);
			this.RaiseAndSetIfChanged(ref _value, v, nameof(Value));
		}
	}

	string FormatValueForDisplay(double val)
	{
		var s = val.ToString();
		//var s = val.ToString("N");
		//Log.Debug($"FormatValueForDisplay {s}");
		return s;

	}

	double FormatDisplayForValue(string display)
	{
		if (!double.TryParse(display,System.Globalization.NumberStyles.Any, null, out var val)) {
			return _value;
			//TODO error handle ?
		}
		else {
			return val;
		}
	}

	void SetDefaultsFromType(IUsageParameter input)
	{
		var it = input.InputType.UnWrapNullable();

		// https://stackoverflow.com/questions/503263/how-to-determine-if-a-type-implements-a-specific-generic-interface-type
		bool isMinMax = it.GetInterfaces().Any(x =>
			x.IsGenericType &&
			x.GetGenericTypeDefinition() == typeof(IMinMaxValue<>)
		);

		if (isMinMax) {
			Min = input.Min ?? Convert.ToDouble(it.GetField(nameof(int.MinValue)).GetValue(null));
			Max = input.Max ?? Convert.ToDouble(it.GetField(nameof(int.MaxValue)).GetValue(null));
		}
		else {
			throw Squeal.NotSupported($"Type {it.Name}");
		}

		Value = input.Default == null ? 0.0 : Convert.ToDouble(input.Default);
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
