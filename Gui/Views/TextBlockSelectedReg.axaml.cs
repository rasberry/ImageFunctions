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

		//this.Bind(context, vm => vm.Selected.Name, v => v.Text);
		//this.Bind(context.Selected.Name, v => this.Text);

		// var top = TopLevel.GetTopLevel(this);
		// Core.Log.Debug($"{top.GetType().FullName} - {top.Content?.GetType()?.FullName}");
		// //var ex = top.FindControl<Expander>($"Reg{NameSpace}");
		// //var rr = top.FindNameScope().Find($"Reg{NameSpace}");
		// var topPanel = top.Content as Panel;
		// var cc = FindControlByName(topPanel,$"Reg{NameSpace}");
		// //Core.Log.Debug($"Expander ex={(ex==null?"null":"good")} cc={(cc==null?"null":"good")} rr={(rr==null?"null":"good")}");
		// Core.Log.Debug($"Expander cc={(cc==null?"null":"good")} {cc?.GetType().Name}");

		// var dc = cc.DataContext as SelectionViewModel;
		// Core.Log.Debug($"sel {dc?.Selected?.Name}");

		// //Expander uL = this.FindNameScope().Find<Expander>($"Reg{NameSpace}");
		// //Core.Log.Debug($"Expander Reg{NameSpace} - {uL?.Name} isnull={(uL==null?"null":"good")}");

		// //complex bind.. as far as I can tell not possible with normal bind syntax.
		// string bind = $"#Reg{NameSpace}.(({nameof(SelectionViewModel)})DataContext).Selected.Name";
		// //Log.Debug($"{nameof(TextBlockSelectedReg)} - {bind}");

		// var b = new Binding(bind) {
		// 	NameScope = new WeakReference<INameScope>(cc.FindNameScope())
		// };
		// b.TypeResolver += (ns,name) => {
		// 	if (name == nameof(SelectionViewModel)) {
		// 		return typeof(SelectionViewModel);
		// 	}
		// 	throw new NotSupportedException(name);
		// };
		// b.WhenAnyValue(v => v).Subscribe(v => Core.Log.Debug(v.ToString()));
		// this.Bind(TextProperty, b);
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
