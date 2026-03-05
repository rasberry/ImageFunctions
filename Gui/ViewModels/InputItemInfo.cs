using ImageFunctions.Core;

namespace ImageFunctions.Gui.ViewModels;

public class InputItemInfo : InputItem
{
	public InputItemInfo(IUsageText input, IEnumerable<string> lines) : base(input)
	{
		CombinedInfo = String.Join('\n', lines);
	}

	public string CombinedInfo { get; init; }
}
