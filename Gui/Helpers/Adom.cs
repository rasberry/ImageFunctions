using Avalonia.Controls;

namespace ImageFunctions.Gui.Helpers;

public static class Adom
{
	public static Control FindControlByNameFromTop(this Control parent, string name)
	{
		var top = TopLevel.GetTopLevel(parent);
		return FindControlByName(top, name);
	}

	public static Control FindControlByName(this Control parent, string name)
	{
		//var found = parent.FindControl<Control>(name);
		//Core.Log.Debug($"Found {found?.GetType().Name} : {found?.Name}");
		//return found;

		//var indent = new String('|',level).Replace("|"," |");
		//Core.Log.Debug($"Find{indent}{parent?.GetType().Name} : {parent?.Name}");

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