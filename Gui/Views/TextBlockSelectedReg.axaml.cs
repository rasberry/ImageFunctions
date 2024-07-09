using Avalonia;
using Avalonia.Controls;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.ViewModels;
using ReactiveUI;

namespace ImageFunctions.Gui.Views;

public partial class TextBlockSelectedReg : TextBlock
{
	static TextBlockSelectedReg()
	{
		NameSpaceProperty = AvaloniaProperty.Register<TextBlockSelectedReg, string>("NameSpace");
		IsConnectedProperty = AvaloniaProperty.Register<TextBlockSelectedReg, bool>("IsConnected");
	}

	public TextBlockSelectedReg() : base()
	{
		Initialized += Init;
	}

	void Init(object sender, EventArgs args)
	{
		if (String.IsNullOrWhiteSpace(NameSpace)) {
			throw new ArgumentNullException(nameof(NameSpace));
		}

		Text = Program.Register.Default(NameSpace);

		var regControl = this.FindControlByNameFromTop($"Reg{NameSpace}");
		if (regControl == null) {
			throw new ArgumentNullException($"Reg{NameSpace}");
		}

		var context = (SelectionViewModel)regControl.DataContext;
		context.WhenAnyValue(v => v.Selected.Name)
			.Subscribe(SetTextWhenConnected);

		this.WhenAnyValue(v => v.IsConnected)
			.Subscribe(v => SetTextWhenConnected(context.Selected?.Name));
	}

	void SetTextWhenConnected(string text)
	{
		if (this.IsConnected) {
			this.Text = text;
		}
	}

	public string NameSpace
	{
		get => GetValue(NameSpaceProperty);
		set => SetValue(NameSpaceProperty, value);
	}

	public bool IsConnected
	{
		get => GetValue(IsConnectedProperty);
		set => SetValue(IsConnectedProperty, value);
	}

	public static readonly StyledProperty<string> NameSpaceProperty;
	public static readonly StyledProperty<bool> IsConnectedProperty;
}
