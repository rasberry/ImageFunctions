using System.Collections.ObjectModel;
using Avalonia.Controls;
using System.Collections;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class SelectionItem
{
	public string Name { get; init; }
}

public class SelectionItemColor : SelectionItem
{
	public Avalonia.Media.Brush Color { get; init; }
}

public partial class SelectionViewModel : ViewModelBase
{
	public SelectionKind Kind { get; set; }
	public ObservableCollection<SelectionItem> Items { get; set; }

	string _selectedText;
	public string SelectedText {
		get => String.IsNullOrWhiteSpace(_selectedText) ? "" : $"- {_selectedText}";
		set => this.RaiseAndSetIfChanged(ref _selectedText, value);
	}

	SelectionItem _selected;
	public SelectionItem Selected {
		get => _selected;
		set => this.RaiseAndSetIfChanged(ref _selected, value);
	}

	public void ItemSelected(object sender, SelectionChangedEventArgs args)
	{
		args.Handled = true;
		var addedRaw = GetFirst(args.AddedItems);
		if (addedRaw is SelectionItem added) {
			// System.Diagnostics.Trace.WriteLine($"Selected {Kind} {added.Name}");
			SelectedText = added.Name;
			Selected = added;
		}
	}

	//no (direct) linq way of doing this ..?
	static object GetFirst(IList list)
	{
		if (list != null && list.Count > 0) {
			return list[0];
		}
		return null;
	}
}

public enum SelectionKind
{
	None = 0,
	Functions,
	Colors,
	Engines,
	Metrics,
	Samplers
}
