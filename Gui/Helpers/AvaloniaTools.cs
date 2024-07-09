using Avalonia.Controls;

namespace ImageFunctions.Gui.Helpers;

public static class AvaloniaTools
{
	public static Control FindControlByNameFromTop(this Control parent, string name)
	{
		var top = TopLevel.GetTopLevel(parent);
		return FindControlByName(top, name);
	}

	public static Control FindControlByName(this Control parent, string name)
	{
		if (parent == null) {
			return null;
		}
		if (parent.Name == name) {
			return parent;
		}
		if (parent is Avalonia.LogicalTree.ILogical il) {
			foreach(var child in il.LogicalChildren) {
				if (child is Control cc) {
					var c = FindControlByName(cc, name);
					if (c != null) { return c; }
				}
			}
		}

		return null;
	}
}