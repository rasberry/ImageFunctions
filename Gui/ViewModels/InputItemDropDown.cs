using ImageFunctions.Core;
using ImageFunctions.Gui.Models;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace ImageFunctions.Gui.ViewModels;

public class InputItemDropDown : InputItem
{
	public InputItemDropDown(IUsageParameter input, IUsageEnum @enum) : base(input)
	{
		var valsList = Rasberry.Cli.PrintHelper.EnumAll(@enum.EnumType, @enum.ExcludeZero);

		int selIndex = 0;
		foreach(var item in valsList) {
			string num = ((int)item).ToString();
			var name = @enum.NameMap != null ? @enum.NameMap(item) : item.ToString();
			var tag = @enum.DescriptionMap != null ? @enum.DescriptionMap(item) : null;

			var sel = new SelectionItem() { Name = $"{num}. {name}", Tag = tag, Value = item };
			Choices.Add(sel);

			if(input.Default != null && input.Default.Equals(item)) {
				SelectedIndex = selIndex;
			}
			selIndex++;
		}
	}

	public InputItemDropDown(IUsageParameter input, IEnumerable<string> @enum) : base(input)
	{
		int selIndex = 0;
		foreach(var name in @enum) {
			var sel = new SelectionItem() { Name = name, Value = name };
			Choices.Add(sel);

			if(input.Default != null && input.Default.Equals(name)) {
				SelectedIndex = selIndex;
			}
			selIndex++;
		}
	}

	public ObservableCollection<SelectionItem> Choices { get; init; } = new();

	int _selectedIndex;
	public int SelectedIndex {
		get => _selectedIndex;
		set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
	}
}
