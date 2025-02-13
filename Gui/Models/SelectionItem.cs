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
	public Avalonia.Media.Brush Color { get; init; }
}
