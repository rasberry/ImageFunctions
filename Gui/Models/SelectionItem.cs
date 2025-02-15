using Avalonia.Media;
using ImageFunctions.Core;
using ImageFunctions.Gui.Helpers;

namespace ImageFunctions.Gui.Models;

public class SelectionItem
{
	public string Name { get; init; }
	public string Tag { get; init; }
	public string NameSpace { get; init; }
	public object Value { get; init; }
}

public class SelectionItemColor : SelectionItem
{
	public SolidColorBrush Color {
		get {
			var ac = ((ColorRGBA)Value).ToColor();
			return new SolidColorBrush(ac);
		}
	}
}
