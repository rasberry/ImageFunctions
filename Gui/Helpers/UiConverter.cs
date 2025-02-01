using Avalonia.Data;
using Avalonia.Data.Converters;
using System.Globalization;

namespace ImageFunctions.Gui.Helpers;

internal class UiConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		//Trace.WriteLine($"v={value} in={(value==null?"Y":"n")}) vt={value?.GetType()?.FullName} tt={targetType?.FullName}");
		if (value == null) { return null; }

		//shortcut for no-op conversions
		if (targetType.IsAssignableFrom(value.GetType())) {
			return value;
		}

		var parser = new Rasberry.Cli.DefaultParser(culture);
		try {
			return parser.Parse(targetType, value.ToString());
		}
		catch {
			//special case for nullables - if parsing fails allow an 'empty' value
			if (Nullable.GetUnderlyingType(targetType) != null) {
				return null;
			}

			// converter used for the wrong type
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		//we can do this because convert is bi-directional
		return Convert(value, targetType, parameter, culture);
	}
}