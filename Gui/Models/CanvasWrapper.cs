using Avalonia.Threading;
using ImageFunctions.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImageFunctions.Gui.Models;

public class CanvasWrapper : ICanvas, INotifyPropertyChanged
{
	public CanvasWrapper(ICanvas canvas)
	{
		Canvas = canvas;
	}

	readonly ICanvas Canvas;

	[IndexerName("Item")]
	public ColorRGBA this[int x, int y] {
		get {
			return Canvas[x, y];
		}
		set {
			//not firing notifications in here because it seems to cause deadlocks
			if(!_isDirty) {
				//Trace.WriteLine($"{nameof(CanvasWrapper)} Set {this.GetHashCode()}");
				_isDirty = true;
			}
			Canvas[x, y] = value;
		}
	}

	public int Width { get { return Canvas.Width; } }
	public int Height { get { return Canvas.Height; } }

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
			Dispatcher.UIThread.Post(() => {
				var e = new PropertyChangedEventArgs(nameof(IsDirty));
				PropertyChanged?.Invoke(this, e);
			});
		}
	}

	public void DeclareClean()
	{
		//Trace.WriteLine($"{nameof(CanvasWrapper)} {nameof(DeclareClean)} {this.GetHashCode()}");
		//fire notifications since this function should only be run after the IFunction Run command is finished
		IsDirty = false;
	}

	public event PropertyChangedEventHandler PropertyChanged;
}
