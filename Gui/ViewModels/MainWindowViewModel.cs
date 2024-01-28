using Avalonia;
using Avalonia.Styling;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using ReactiveUI;
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
		RegistrationItems = new();

		var functionReg = new FunctionRegister(Program.Register);
		AddTreeNodeFromRegistered(functionReg, "Functions", (reg, name) => {
			return new TreeNode { Name = name };
		});

		var colorReg = new ColorRegister(Program.Register);
		AddTreeNodeFromRegistered(colorReg, "Colors", (reg, name) => {
			return new ColorTreeNode {
				Name = name,
				Color = ConvertColor(name,colorReg)
			};
		});

		var engineReg = new EngineRegister(Program.Register);
		AddTreeNodeFromRegistered(engineReg, "Engines", (reg, name) => {
			return new TreeNode { Name = name };
		});

		var metricReg = new MetricRegister(Program.Register);
		AddTreeNodeFromRegistered(metricReg,"Metrics", (reg, name) => {
			return new TreeNode { Name = name };
		});

		var samplerReg = new SamplerRegister(Program.Register);
		AddTreeNodeFromRegistered(samplerReg,"Samplers", (reg, name) => {
			return new TreeNode { Name = name };
		});
	}

	void AddTreeNodeFromRegistered<T>(AbstractRegistrant<T> reg, string title, Func<AbstractRegistrant<T>,string,TreeNode> filler)
	{
		var node = new TreeNode {
			Name = title,
			Items = new()
		};
		foreach(var c in reg.All().OrderBy(n => n)) {
			var item = filler(reg,c);
			node.Items.Add(item);
		}
		RegistrationItems.Add(node);
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

	public ObservableCollection<TreeNode> RegistrationItems { get; private set; }
}

public class TreeNode
{
	public ObservableCollection<TreeNode> Items { get; init; }
	public string Name { get; init; }
}

public class ColorTreeNode : TreeNode
{
	public Avalonia.Media.Brush Color { get; init; }
}
