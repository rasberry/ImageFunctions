#if false
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ImageFunctions.Core;
using System.Globalization;

namespace ImageFunctions.Gui.Helpers;

public class ColorToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType.Equals(typeof(Brush))) {
			if (value is ColorRGBA rgba) {
				var ac = rgba.ToColor();
				return new SolidColorBrush(ac);
			}
			else if (value is Color native) {
				return new SolidColorBrush(native);
			}
		}

		return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
#endif