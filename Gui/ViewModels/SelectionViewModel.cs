using Avalonia.Controls;
using ImageFunctions.Gui.Models;
using ReactiveUI;
using System.Collections;
using System.Collections.ObjectModel;

namespace ImageFunctions.Gui.ViewModels;

public partial class SelectionViewModel : ViewModelBase
{
	public string NameSpace { get; set; }
	public ObservableCollection<SelectionItem> Items { get; set; }

	SelectionItem _selected;
	public SelectionItem Selected {
		get => _selected;
		set => this.RaiseAndSetIfChanged(ref _selected, value);
	}
}
