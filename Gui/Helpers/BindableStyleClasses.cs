using Avalonia;

namespace ImageFunctions.Gui.Helpers;

// https://github.com/AvaloniaUI/Avalonia/issues/2427#issuecomment-1210469405
// https://github.com/Noemata/nodify-avalonia/blob/dc323131eae5240ad0b911961b594531f7530cc8/Nodify/Helpers/BindableStyleClasses.cs
internal class BindableStyleClasses
{
	static BindableStyleClasses()
	{
		ClassesProperty.Changed.AddClassHandler<StyledElement>(HandleClassesChanged);
	}

	public static readonly AttachedProperty<object> ClassesProperty =
		AvaloniaProperty.RegisterAttached<BindableStyleClasses, StyledElement, object>("Classes");

	private static void HandleClassesChanged(StyledElement element, AvaloniaPropertyChangedEventArgs args)
	{
		//Trace.WriteLine($"{nameof(HandleClassesChanged)} c={args.NewValue}");
		element.Classes.Clear();
		if(args.NewValue != null) {
			element.Classes.Add(args.NewValue.ToString());
		}
	}

	public static void SetClasses(StyledElement element, object classes)
	{
		//Trace.WriteLine($"{nameof(SetClasses)}");
		element.SetValue(ClassesProperty, classes);
	}

	public static object GetClasses(StyledElement element)
	{
		//Trace.WriteLine($"{nameof(GetClasses)}");
		return element.GetValue(ClassesProperty);
	}
}
