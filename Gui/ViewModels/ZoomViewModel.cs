using ReactiveUI;
using System.Collections.ObjectModel;

namespace ImageFunctions.Gui.ViewModels;

public class ZoomViewModel : ViewModelBase
{
	int _index = NormalZoomIndex;
	public int Index {
		get {
			return _index;
		}
		set {
			bool updated = _index != value;
			if (updated) {
				this.RaisePropertyChanging(nameof(Index));
				this.RaisePropertyChanging(nameof(Zoom));
				_index = value;
				this.RaisePropertyChanged(nameof(Index));
				this.RaisePropertyChanged(nameof(Zoom));
			}
		}
	}

	public double Zoom {
		get {
			return LevelsList[Index];
		}
	}

	public void Smaller()
	{
		Index = Math.Clamp(Index + 1, 0, LevelsList.Length - 1);
	}

	public void Bigger()
	{
		Index = Math.Clamp(Index - 1, 0, LevelsList.Length - 1);
	}

	public void Reset()
	{
		Index = NormalZoomIndex;
	}

	public static ReadOnlyCollection<double> Levels {
		get {
			return LevelsList.AsReadOnly();
		}
	}

	// static int FindNearest(double zoom)
	// {
	// 	if(zoom >= Levels[0]) { return 0; }
	// 	if(zoom <= Levels[^1]) { return Levels.Length - 1; }

	// 	for(int d = Levels.Length - 2; d >= 0; d--) {
	// 		if(zoom < Levels[d]) {
	// 			double diffup = Levels[d] - zoom; //we know zoom is less than
	// 			double diffdn = zoom - Levels[d - 1]; //we know zoom is greater or equal
	// 			return diffup < diffdn ? d : d - 1;
	// 		}
	// 	}

	// 	//this should never happen
	// 	throw new InvalidOperationException($"Failed to find nearest zoom for {zoom}");
	// }

#pragma warning disable format

	// //these were copied derived from Gimp
	// const int NormalZoomIndex = 16;
	// static readonly double[] Levels = new double[] {
	// 	256.0, 180.0, 128.0,  90.0,  64.0,  45.0,
	// 	32.0,   23.0,  16.0,  11.0,   8.0,   5.5,
	// 	 4.0,    3.0,   2.0,   1.5,
	// 	 1.0,
	// 	 1/1.5,  1/2.0,  1/3.0,  1/4.0,
	// 	 1/5.5,  1/8.0,  1/11.0, 1/16.0,  1/23.0,  1/32.0,
	// 	 1/45.0, 1/64.0, 1/90.0, 1/128.0, 1/180.0, 1/256.0
	// };

	//these were copied derived from Gimp
	const int NormalZoomIndex = 12;
	static readonly double[] LevelsList = new double[] {
		64.0,  45.0,   32.0,   23.0,  16.0,  11.0,
		 8.0,   5.5,    4.0,    3.0,   2.0,   1.5,
		 1.0,
		1/1.5,  1/2.0,   1/3.0,   1/4.0,  1/5.5,  1/8.0,
		1/11.0, 1/16.0,  1/23.0,  1/32.0, 1/45.0, 1/64.0,
	};

#pragma warning restore format

	// apparently statics needs to be defined in order to avoid null references
	// so this must go after LevelsList
	static ZoomHelperDisplayItem[] _items = InitZoomItems();
	public static ReadOnlyCollection<ZoomHelperDisplayItem> Items {
		get {
			return _items.AsReadOnly();
		}
	}
	static ZoomHelperDisplayItem[] InitZoomItems()
	{
		var items = new ZoomHelperDisplayItem[LevelsList.Length];
		for(int i = 0; i < LevelsList.Length; i++) {
			items[i] = new ZoomHelperDisplayItem(LevelsList[i]);
		}
		return items;
	}

}

public sealed class ZoomHelperDisplayItem
{
	public ZoomHelperDisplayItem(double val)
	{
		Value = val;
	}

	public double Value { get; private set; }
	public string Display {
		get {
			if (Value >= 1.0) {
				return Value.ToString("P0");
			}
			else {
				return Value.ToString("P2");
			}
		}
	}
}
