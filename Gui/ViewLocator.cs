using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ImageFunctions.Core;
using ImageFunctions.Gui.ViewModels;
using ImageFunctions.Gui.Views;

namespace ImageFunctions.Gui;

public class ViewLocator : IDataTemplate
{
	public Control Build(object data)
	{
		//Log.Debug($"ViewLocator.Build data={data?.ToString()}");
		if(data is null) { return null; }

		if(data is LayersImageData) {
			return new LayersImageControl() { DataContext = data };
		}
		else if (data is SelectionViewModel svm) {
			var svmName = $"Reg{svm.NameSpace}";
			return new RegisteredControl { Name = svmName, DataContext = svm };
		}

		var name = data.GetType().FullName.Replace("ViewModel", "Views", StringComparison.Ordinal);
		var type = Type.GetType(name);
		Log.Debug($"ViewLocator.Build Locating {name}");

		if(type != null) {
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
