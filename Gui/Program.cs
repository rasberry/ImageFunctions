﻿using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using ImageFunctions.Core;
using System.Diagnostics;

namespace ImageFunctions.Gui;

internal sealed class Program
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
			//.WithInterFont()
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
		Register = new CoreRegister(Log);
		PluginLoader.LoadAllPlugins(Register, Log);
	}

	static void Cleanup()
	{
		if(Register != null) {
			Register.Dispose();
			Register = null;
		}
	}

	internal static CoreRegister Register { get; private set; }
	internal static LogToConsole Log = new();
}
