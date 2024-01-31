using System.Collections.ObjectModel;
using Avalonia.Controls;
using System.Linq;
using System.Collections;

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

	public void ItemSelected(object sender, SelectionChangedEventArgs args)
	{
		var addedRaw = GetFirst(args.AddedItems);
		if (addedRaw is SelectionItem added) {
			System.Diagnostics.Trace.WriteLine($"Selected {this.Kind} {added.Name}");
		}
	}

	static object GetFirst(IList list)
	{
		if (list != null) { return list[0]; }
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
