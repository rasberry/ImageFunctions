using ImageFunctions.Core;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class InputItemText : InputItem
{
	public InputItemText(IUsageParameter input) : base(input)
	{
		if(input.Default != null) {
			Text = input.Default.ToString();
		}
	}

	string _text;
	public string Text {
		get => _text ?? "";
		set => this.RaiseAndSetIfChanged(ref _text, value);
	}
}