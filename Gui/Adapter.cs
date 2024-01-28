using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using ImageFunctions.Core;

namespace ImageFunctions.Gui;

class Adapter : IPlugin
{
	public void Dispose()
	{
		//throw new NotImplementedException();
	}

	public void Init(IRegister register)
	{
		var reg = new FunctionRegister(register);
		reg.Add(nameof(Gui),GuiFunction.Create);
	}
}

class GuiFunction : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions options)
	{
		var f = new GuiFunction {
			Register = register,
			Core = options,
			Layers = layers
		};
		return f;
	}

	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;

	public bool Run(string[] args)
	{
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
		return true;
	}

	public void Usage(StringBuilder sb)
	{
		sb.AppendLine(" A Graphical Application for running functions");
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	static AppBuilder BuildAvaloniaApp()
	{
		var builder = AppBuilder
			.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.UseReactiveUI();

		#if DEBUG
		builder.LogToTrace(LogEventLevel.Debug, LogArea.Platform);
		#else
		builder.LogToTrace();
		#endif

		return builder;
	}
}