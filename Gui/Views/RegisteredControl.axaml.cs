using Avalonia.Controls;
using Avalonia.LogicalTree;
using ImageFunctions.Gui.Models;
using ImageFunctions.Gui.ViewModels;

namespace ImageFunctions.Gui.Views;

public partial class RegisteredControl : UserControl
{
	public RegisteredControl()
	{
		InitializeComponent();
		Initialized += (s, e) => {
			if(!string.IsNullOrWhiteSpace(SelectItemName)) {
				SelectItem(SelectItemName);
			}
		};
	}

	//TODO selecting the engine this way feels extremely manual.
	// seems like there should be a better way
	void SelectItem(string name)
	{
		var expander = this.FindLogicalDescendantOfType<Expander>();
		var listBox = this.FindLogicalDescendantOfType<ListBox>();

		//have to expand this first or the listBox inside won't initialize
		expander.Initialized += (s, e) => {
			var ex = (Expander)s;
			ex.IsExpanded = true;
			Model.SelectedText = name;
		};

		listBox.Initialized += (s, e) => {
			var lb = (ListBox)s;
			int i = 0;
			foreach(SelectionItem item in lb.Items) {
				if(item.Name == name) {
					lb.SelectedIndex = i;
					break;
				}
				i++;
			}
		};
	}

	//Note: always check for null before using this e.g. Model?.
	SelectionViewModel Model {
		get {
			return DataContext as SelectionViewModel;
		}
	}

	public string SelectItemName { get; set; }

	public void OnItemSelected(object sender, SelectionChangedEventArgs args)
	{
		Model?.ItemSelected(sender, args);
	}
}
