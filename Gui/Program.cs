using System.Diagnostics;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using ImageFunctions.Core;

namespace ImageFunctions.Gui;

sealed class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	static void Main(string[] args)
	{
		//#if DEBUG
		Trace.Listeners.Add(new ConsoleTraceListener());
		//#endif

		//??? TODO why is this needed suddenly -- apparently <Private>false</Private> ???
		/*System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += (s,e) => {
			var path = typeof(Program).Assembly.Location;
			string dllName = $"{e.Name}.dll";
			string full = Path.Combine(Path.GetDirectoryName(path),dllName);
			Trace.WriteLine($"{full}");
			if (File.Exists(full)) {
				return s.LoadFromAssemblyPath(full);
			}
			return null;
		};*/

		try {
			PluginSetup();
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
		}
		finally {
			Cleanup();
		}

		//#if DEBUG
		Trace.Flush();
		//#endif
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

	static void PluginSetup()
	{
		Trace.WriteLine("PluginSetup");
		Register = new Register();
		PluginLoader.LoadAllPlugins(Register);
	}

	public static void Cleanup()
	{
		if (Register != null) {
			Register.Dispose();
			Register = null;
		}
	}

	static Register Register;
}