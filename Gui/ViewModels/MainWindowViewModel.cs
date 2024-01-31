using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using DynamicData.Binding;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ImageFunctions.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	internal MainWindowViewModel()
	{
		RxApp.MainThreadScheduler.Schedule(LoadData);
	}

	void LoadData()
	{
		var functionReg = new FunctionRegister(Program.Register);
		RegFunctionItems = AddTreeNodeFromRegistered(SelectionKind.Functions, functionReg, (reg, name) => {
			return new SelectionItem { Name = name };
		}, OnFunctionSelected);

		var colorReg = new ColorRegister(Program.Register);
		RegColorItems = AddTreeNodeFromRegistered(SelectionKind.Colors, colorReg, (reg, name) => {
			return new SelectionItemColor {
				Name = name,
				Color = ConvertColor(name,colorReg)
			};
		}, OnSomethingSelected);

		var engineReg = new EngineRegister(Program.Register);
		RegEngineItems = AddTreeNodeFromRegistered(SelectionKind.Engines, engineReg, (reg, name) => {
			return new SelectionItem { Name = name };
		}, OnSomethingSelected);

		var metricReg = new MetricRegister(Program.Register);
		RegMetricItems = AddTreeNodeFromRegistered(SelectionKind.Metrics, metricReg, (reg, name) => {
			return new SelectionItem { Name = name };
		}, OnSomethingSelected);

		var samplerReg = new SamplerRegister(Program.Register);
		RegSamplerItems = AddTreeNodeFromRegistered(SelectionKind.Samplers, samplerReg, (reg, name) => {
			return new SelectionItem { Name = name };
		}, OnSomethingSelected);
	}

	SelectionViewModel AddTreeNodeFromRegistered<T>(SelectionKind Kind,
		AbstractRegistrant<T> reg,
		Func<AbstractRegistrant<T>,string,SelectionItem> filler,
		Action<SelectionItem> selectionHandler

	) {
		var items = new ObservableCollection<SelectionItem>();
		foreach(var c in reg.All().OrderBy(n => n)) {
			var item = filler(reg,c);
			items.Add(item);
		}
		var sel = new SelectionViewModel {
			Kind = Kind,
			Items = items
		};

		sel.WhenAnyValue(p => p.Selected)
			.Subscribe(selectionHandler);

		return sel;
	}

	//TODO switch to ObservableCollection<SelectionItem> maybe?
	public SelectionViewModel RegColorItems     { get; private set; }
	public SelectionViewModel RegEngineItems    { get; private set; }
	public SelectionViewModel RegFunctionItems  { get; private set; }
	public SelectionViewModel RegMetricItems    { get; private set; }
	public SelectionViewModel RegSamplerItems   { get; private set; }

	void OnFunctionSelected(SelectionItem item)
	{
		if (item == null) { return; }
		var reg = new FunctionRegister(Program.Register);
		if (!reg.Try(item.Name, out var regItem)) {
			Trace.WriteLine($"Function {item.Name} should have been found but wasn't !!");
			return;
		}

		//new Core.Options
		//regItem.Item.Invoke();
	}

	void OnSomethingSelected(SelectionItem item)
	{
		if (item == null) { return; }
		Trace.WriteLine($"OnSomethingSelected {item.Name}");
	}

	static Avalonia.Media.Brush ConvertColor(string key, ColorRegister reg)
	{
		var c = reg.Get(key).Item;
		var ac = Avalonia.Media.Color.FromArgb(
			(byte)(c.A * 255.0),
			(byte)(c.R * 255.0),
			(byte)(c.G * 255.0),
			(byte)(c.B * 255.0)
		);
		return new Avalonia.Media.SolidColorBrush(ac);
	}

	string StatusTextValue = $"Welcome to {nameof(ImageFunctions)}";
	public string StatusText {
		get =>  StatusTextValue;
		set => this.RaiseAndSetIfChanged(ref StatusTextValue, value);
	}

	string CommandTextValue = "";
	public string CommandText {
		get => CommandTextValue;
		set => this.RaiseAndSetIfChanged(ref CommandTextValue, value);
	}

	string UsageTextValue = "";
	public string UsageText {
		get => UsageTextValue;
		set => this.RaiseAndSetIfChanged(ref UsageTextValue, value);
	}

	public bool ToggleThemeClick()
	{
		var app = Application.Current;
		if (app is not null) {
			var theme = app.ActualThemeVariant;
			app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
		}
		return true;
	}

	System.Timers.Timer StatusTextTimer = null;
	const int StatusTextLifetimeMs = 2000;

	// The behavior shows the text as long as the control is still under the pointer
	// but wait before hiding the text after the pointer leaves
	public void UpdateStatusText(string text = "", bool startTimer = false)
	{
		//Trace.WriteLine($"UpdateStatusText T:'{text}' E:{(expired?"Y":"N")}");
		if (StatusTextTimer == null) {
			StatusTextTimer = new() {
				AutoReset = false,
				Interval = StatusTextLifetimeMs
			};
			//this clears the status after some time
			StatusTextTimer.Elapsed += (s,e) => UpdateStatusText("",false);
		}

		StatusText = text;

		if (startTimer) {
			StatusTextTimer.Start();
		}
		else {
			StatusTextTimer.Stop();
		}
	}
}
