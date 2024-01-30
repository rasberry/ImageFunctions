using Avalonia;
using Avalonia.Styling;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using ReactiveUI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;

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
		RegFunctionItems = AddTreeNodeFromRegistered("Functions", functionReg, (reg, name) => {
			return new SelectionItem { Name = name };
		});

		var colorReg = new ColorRegister(Program.Register);
		RegColorItems = AddTreeNodeFromRegistered("Colors", colorReg, (reg, name) => {
			return new SelectionItemColor {
				Name = name,
				Color = ConvertColor(name,colorReg)
			};
		});

		var engineReg = new EngineRegister(Program.Register);
		RegEngineItems = AddTreeNodeFromRegistered("Engines", engineReg, (reg, name) => {
			return new SelectionItem { Name = name };
		});

		var metricReg = new MetricRegister(Program.Register);
		RegMetricItems = AddTreeNodeFromRegistered("Metrics", metricReg, (reg, name) => {
			return new SelectionItem { Name = name };
		});

		var samplerReg = new SamplerRegister(Program.Register);
		RegSamplerItems = AddTreeNodeFromRegistered("Samplers", samplerReg, (reg, name) => {
			return new SelectionItem { Name = name };
		});
	}

	RegisteredSelection AddTreeNodeFromRegistered<T>(string Name, AbstractRegistrant<T> reg, Func<AbstractRegistrant<T>,string,SelectionItem> filler)
	{
		var items = new ObservableCollection<SelectionItem>();
		foreach(var c in reg.All().OrderBy(n => n)) {
			var item = filler(reg,c);
			items.Add(item);
		}
		var sel = new RegisteredSelection {
			Name = Name,
			Items = items
		};

		return sel;
	}

	public RegisteredSelection RegColorItems     { get; private set; }
	public RegisteredSelection RegEngineItems    { get; private set; }
	public RegisteredSelection RegFunctionItems  { get; private set; }
	public RegisteredSelection RegMetricItems    { get; private set; }
	public RegisteredSelection RegSamplerItems   { get; private set; }

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

	// The behavior is to show the text as long as the control is still under the pointer
	// but wait some time before hiding the text after the pointer leaves
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

public class RegisteredSelection
{
	public string Name { get; init; }
	public ObservableCollection<SelectionItem> Items { get; init; } = new();
}

public class SelectionItem
{
	public string Name { get; init; }
}

public class SelectionItemColor : SelectionItem
{
	public Avalonia.Media.Brush Color { get; init; }
}
