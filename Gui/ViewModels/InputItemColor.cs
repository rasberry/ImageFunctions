using Avalonia.Media;
using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.Models;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public sealed class InputItemColor : InputItemSync
{
	public InputItemColor(IUsageParameter input, SelectionViewModel model) : base(input, model)
	{
		SubItem = this.WhenAnyValue(v => v.Item).Subscribe(SetColorFromItem);

		if(input.Default != null) {
			if(input.Default is Color native) {
				Color = native;
			}
			else if(input.Default is ColorRGBA rgba) {
				Color = rgba.ToColor();
			}
			else if(input.Default is System.Drawing.Color sdcolor) {
				Color = Color.FromArgb(sdcolor.A, sdcolor.R, sdcolor.G, sdcolor.B);
			}
			else {
				var typeName = input.Default.GetType().FullName;
				throw Squeal.NotSupported($"Color Type {typeName}");
			}
		}
	}

	public static bool IsSupportedColorType(Type it)
	{
		return it.Is<ColorRGBA>() || it.Is<Color>() || it.Is<System.Drawing.Color>();
	}

	void SetColorFromItem(SelectionItem item)
	{
		if(item == null) { return; }
		//Trace.WriteLine($"SetValueFromItem {item.Name} - {item.Value}");
		Color = ((ColorRGBA)item.Value).ToColor();
	}

	readonly IDisposable SubItem;

	public override void Dispose()
	{
		base.Dispose();
		SubItem?.Dispose();
	}

	Color _color;
	public Color Color {
		get => _color;
		set => this.RaiseAndSetIfChanged(ref _color, value);
	}
}