using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ImageFunctions.Gui.ViewModels;
using ImageFunctions.Gui.Views;

namespace ImageFunctions.Gui;

public class ViewLocator : IDataTemplate
{
	public Control Build(object data)
	{
		Trace.WriteLine($"ViewLocator.Build data={data?.ToString()}");
		if (data is null) { return null; }

		if (data is LayersImageData) {
			return new LayersImageControl() { DataContext = data };
		}

		var name = data.GetType().FullName.Replace("ViewModel", "Views", StringComparison.Ordinal);
		var type = Type.GetType(name);
		Trace.WriteLine($"ViewLocator.Build Locating {name}");

		if (type != null) {
			var control = (Control)Activator.CreateInstance(type);
			control.DataContext = data;
			return control;
		}

		return new TextBlock { Text = "Not Found: " + name };
	}

	public bool Match(object data)
	{
		return data is ViewModelBase;
	}
}
