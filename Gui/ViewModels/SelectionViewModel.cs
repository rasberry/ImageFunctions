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

	// public void ItemSelected(object sender, SelectionChangedEventArgs args)
	// {
	// 	args.Handled = true;
	// 	var addedRaw = GetFirst(args.AddedItems);
	// 	if(addedRaw is SelectionItem added) {
	// 		// System.Diagnostics.Trace.WriteLine($"Selected {Kind} {added.Name}");
	// 		Selected = added;
	// 	}
	// }

	// //no (direct) linq way of doing this ..?
	// static object GetFirst(IList list)
	// {
	// 	if(list != null && list.Count > 0) {
	// 		return list[0];
	// 	}
	// 	return null;
	// }
}
