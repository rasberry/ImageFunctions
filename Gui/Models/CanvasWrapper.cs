using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageFunctions.Core;

namespace ImageFunctions.Gui.Models;

public class CanvasWrapper : ICanvas, INotifyPropertyChanged
{
	public CanvasWrapper(ICanvas canvas)
	{
		Canvas = canvas;
	}

	readonly ICanvas Canvas;

	[IndexerName ("Item")]
	public ColorRGBA this[int x, int y] {
		get {
			return Canvas[x,y];
		}
		set {
			if (!IsDirty) { IsDirty = true; }
			Canvas[x,y] = value;
			// https://stackoverflow.com/questions/657675/propertychanged-for-indexer-property
			var e = new PropertyChangedEventArgs("Item[]");
			PropertyChanged.Invoke(this, e);
		}
	}

	public int Width { get { return Canvas.Width; }}
	public int Height { get { return Canvas.Height; }}

	public void Dispose()
	{
		Canvas?.Dispose();
		GC.SuppressFinalize(this);
	}

	bool _isDirty = false;
	public bool IsDirty {
		get {
			return _isDirty;
		}
		set {
			_isDirty = value;
			var e = new PropertyChangedEventArgs(nameof(IsDirty));
			PropertyChanged.Invoke(this, e);
		}
	}

	public void DeclareClean() {
		IsDirty = false;
	}

	public event PropertyChangedEventHandler PropertyChanged;
}