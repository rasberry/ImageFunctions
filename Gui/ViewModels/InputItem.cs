using Avalonia.Media;
using ImageFunctions.Core;
using ImageFunctions.Gui.Helpers;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class InputItem : ViewModelBase
{
	static InputItem()
	{
		IconMultiAdd = AvaloniaTools.GetIconFromName("IconPlusCircle");
		IconMultiRemove = AvaloniaTools.GetIconFromName("IconCloseCircle");
	}

	static readonly StreamGeometry IconMultiAdd;
	static readonly StreamGeometry IconMultiRemove;

	public InputItem(IUsageText input)
	{
		Input = input;
	}

	public IUsageText Input { get; init; }
	public string Description { get { return Input.Description; } }
	public string Name { get { return Input.Name; } }

	// alternate name feature
	public string Alternate { get; init; }

	//multiple of same parameter feature
	public bool MultipleEnabled { get { return AddOrRemoveHandler != null; } }
	public bool IsMultiplePrimary { get { return MultipleIndex == 0; } }
	public Action<InputItem> AddOrRemoveHandler { get; init; }
	public int MultipleIndex { get; init; }

	public string NameDisplay {
		get {
			return String.IsNullOrWhiteSpace(Alternate) ? Name : $"{Name} / {Alternate}";
		}
	}

	public string MultiButtonTag {
		get {
			return IsMultiplePrimary ? "Add another of this parameter" : "Remove this parameter";
		}
	}

	public StreamGeometry MultiIcon {
		get {
			return IsMultiplePrimary ? IconMultiAdd : IconMultiRemove;
		}
	}

	//enabled means the input item has been checked in the ui
	bool _enabled;
	public bool Enabled {
		get => _enabled;
		set => this.RaiseAndSetIfChanged(ref _enabled, value);
	}

	public void AddOrRemoveInputItem()
	{
		//Trace.WriteLine("CloneOrRemoveInputItem");
		AddOrRemoveHandler?.Invoke(this);
	}
}
