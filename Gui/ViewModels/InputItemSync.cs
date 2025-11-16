using Avalonia.Media;
using ImageFunctions.Core;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.Models;
using ReactiveUI;


namespace ImageFunctions.Gui.ViewModels;

public class InputItemSync : InputItem, IDisposable
{
	static InputItemSync()
	{
		IconSyncData = AvaloniaTools.GetIconFromName("IconSync");
		IconSyncOffData = AvaloniaTools.GetIconFromName("IconSyncOff");
	}

	static readonly StreamGeometry IconSyncData;
	static readonly StreamGeometry IconSyncOffData;

	public InputItemSync(IUsageParameter input, SelectionViewModel svModel) : base(input)
	{
		var reg = Program.Register;
		NameSpace = svModel.NameSpace;

		//when the Registered item selection changes, update Item
		SubSelected = svModel.WhenAnyValue(v => v.Selected)
			.Subscribe(SetItemWhenConnected);
		SubIsSyncEnabled = this.WhenAnyValue(v => v.IsSyncEnabled)
			.Subscribe(s => SetItemWhenConnected(svModel.Selected));

		var defName = reg.Default(NameSpace);
		if(!String.IsNullOrEmpty(defName)) {
			Item = new SelectionItem { Name = defName, NameSpace = NameSpace, Value = defName };
		}
		else {
			//without a default the controls don't work. I can't explain..
			throw Squeal.ArgumentNullOrEmpty($"Missing default for {NameSpace}");
		}
		SetSyncIcon();
	}

	void SetItemWhenConnected(SelectionItem item)
	{
		if(item == null) { return; }
		if(this.IsSyncEnabled) {
			this.Item = item;
		}
	}

	public string NameSpace { get; private set; }
	public string Tag { get { return $"Synchronize with {NameSpace}"; } }

	StreamGeometry _syncIcon;
	public StreamGeometry SyncIcon {
		get => _syncIcon;
		set => this.RaiseAndSetIfChanged(ref _syncIcon, value);
	}

	void SetSyncIcon()
	{
		SyncIcon = IsSyncEnabled ? IconSyncData : IconSyncOffData;
	}

	readonly IDisposable SubSelected;
	readonly IDisposable SubIsSyncEnabled;

	public virtual void Dispose()
	{
		SubSelected?.Dispose();
		SubIsSyncEnabled?.Dispose();
		GC.SuppressFinalize(this);
	}

	bool _isSyncEnabled;
	public bool IsSyncEnabled {
		get { return _isSyncEnabled; }
		set {
			this.RaiseAndSetIfChanged(ref _isSyncEnabled, value);
			SetSyncIcon();
		}
	}

	SelectionItem _item;
	public SelectionItem Item {
		get => _item;
		set => this.RaiseAndSetIfChanged(ref _item, value);
	}
}