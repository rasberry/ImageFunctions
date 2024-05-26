namespace ImageFunctions.Gui.Models;

public class SelectionItem
{
	public string Name { get; init; }
}

public class SelectionItemColor : SelectionItem
{
	public Avalonia.Media.Brush Color { get; init; }
}
