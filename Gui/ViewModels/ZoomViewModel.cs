using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ImageFunctions.Gui.ViewModels;

public class ZoomViewModel : ViewModelBase
{
	int _index = NormalZoomIndex;
	public int Index {
		get => _index;
		set => DoZoom(value);
	}

	public double Zoom => LevelsList[Index];

	Avalonia.Vector _offset;
	public Avalonia.Vector Offset {
		get => _offset;
		set => this.RaiseAndSetIfChanged(ref _offset, value);
	}

	public Avalonia.Size ViewPort { get; set; }
	public Avalonia.Size Extent { get; set; }

	public void Smaller()
	{
		var ix = Math.Clamp(Index + 1, 0, LevelsList.Length - 1);
		DoZoom(ix);
	}

	public void Bigger()
	{
		var ix = Math.Clamp(Index - 1, 0, LevelsList.Length - 1);
		Trace.WriteLine($"Bigger b={Index} a={ix}");
		DoZoom(ix);
	}

	public void Reset()
	{
		DoZoom(NormalZoomIndex);
	}

	void DoZoom(int newIndex)
	{
		int oldIndex = Index;
		if(oldIndex == newIndex) {
			return;
		}

		Avalonia.Vector oldOffset = Offset;
		double oldZoom = Levels[oldIndex];
		double newZoom = Levels[newIndex];
		var vp = ViewPort;
		var ex = Extent;

		//calculate the new offset to retain center of viewport
		double zoomRatio = newZoom / oldZoom;

		// double sxBarOld = vp.Width / ex.Width * vp.Width / 2;
		// double syBarOld = vp.Height / ex.Height * vp.Height / 2;
		// double sxBarNew = vp.Width / (ex.Width * zoomRatio) * vp.Width / 2;
		// double syBarNew = vp.Height / (ex.Height * zoomRatio) * vp.Height / 2;

		// Avalonia.Vector newOffset = new(
		// 	(oldOffset.X + sxBarOld) * zoomRatio - sxBarNew,
		// 	(oldOffset.Y + syBarOld) * zoomRatio - syBarNew
		// );

		Avalonia.Vector newOffset = new(
			CalcNewOffset(zoomRatio, vp.Width, ex.Width, oldOffset.X),
			CalcNewOffset(zoomRatio, vp.Height, ex.Height, oldOffset.Y)
		);

		//Trace.WriteLine($"DoZoom oz={oldZoom} nz={newZoom} vp={vp} no={newOffset}");

		this.RaisePropertyChanging(nameof(Index));
		this.RaisePropertyChanging(nameof(Zoom));
		this.RaisePropertyChanging(nameof(Offset));
		_index = newIndex;
		_offset = newOffset;
		this.RaisePropertyChanged(nameof(Index));
		this.RaisePropertyChanged(nameof(Zoom));
		this.RaisePropertyChanging(nameof(Offset));
	}

	//r = zoom ratio, v = viewport, e = extenet, x = current scroll offset
	double CalcNewOffset(double r, double v, double e, double x)
	{
		return (r * r - 1) * v * v / (2 * e * r) + r * x;
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

	// //these were derived from Gimp
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

	//these were derived from Gimp
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
			if(Value >= 1.0) {
				return Value.ToString("P0");
			}
			else {
				return Value.ToString("P2");
			}
		}
	}
}
