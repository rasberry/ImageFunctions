using Avalonia;
using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public sealed class InputItemPoint : InputItem, IDisposable
{
	public InputItemPoint(IUsageParameter input, MainWindowViewModel mwvm) : base(input)
	{
		MWVModel = mwvm;

		if(input.Default != null) {
			Point point;
			if(input.Default is Point native) {
				point = native;
			}
			else if(input.Default is Core.PointD cpd) {
				point = new Point(cpd.X, cpd.Y);
			}
			else if(input.Default is System.Drawing.Point sdp) {
				point = new Point(sdp.X, sdp.Y);
			}
			else if(input.Default is System.Drawing.PointF sdpf) {
				point = new Point(sdpf.X, sdpf.Y);
			}
			else {
				var typeName = input.Default.GetType().FullName;
				throw Squeal.NotSupported($"Point Type {typeName}");
			}

			PickedPoint = point;
		}

		if(input.InputType != null) {
			var it = input.InputType;
			if(it.Is<System.Drawing.PointF>()) {
				CoordType = typeof(float);
			}
			else if(it.Is<System.Drawing.Point>()) {
				CoordType = typeof(int);
			}
			//covers Avalonia.Point and Core.PointD
			else {
				CoordType = typeof(double);
			}
		}
	}

	readonly Type CoordType;
	readonly MainWindowViewModel MWVModel;

	bool _isPicking;
	public bool IsPicking {
		get {
			return _isPicking;
		}
		set {
			MWVModel.IsPickingFromPreview = value;
			SetIsPickingAlone(value);
		}
	}

	//seperate function to avoid an infinite event loop with IsPickingFromPreview
	void SetIsPickingAlone(bool val)
	{
		if(val) {
			PickingSubscribe();
		}
		else {
			PickingUnsubscribe();
		}

		if(val != _isPicking) {
			this.RaisePropertyChanging(nameof(IsPicking));
			_isPicking = val;
			this.RaisePropertyChanged(nameof(IsPicking));
		}
	}

	void PickingSubscribe()
	{
		if(IsSubscribed || MWVModel == null) { return; }
		IsSubscribed = true;
		SubPreviewPointerPos = MWVModel.WhenAnyValue(m => m.PreviewPointerPos)
			.Subscribe(p => PickedPoint = p);
		SubIsPickingFromPreview = MWVModel.WhenAnyValue(m => m.IsPickingFromPreview)
			.Subscribe(SetIsPickingAlone);
	}

	void PickingUnsubscribe()
	{
		SubPreviewPointerPos?.Dispose();
		SubIsPickingFromPreview?.Dispose();
		IsSubscribed = false;
	}

	string FormatCoord(double raw)
	{
		if(CoordType.Is<int>()) {
			//spacial case-ing because Convert.ChangeType is rounding the double instead of flooring 🤷
			return ((int)raw).ToString();
		}
		else {
			return Convert.ChangeType(raw, CoordType).ToString();
		}
	}

	public string PickedX {
		get {
			return FormatCoord(PickedPoint.X);
		}
		set {
			var newVal = Convert.ToDouble(value);
			if(PickedPoint.X != newVal) {
				PickedPoint = new Point(newVal, PickedPoint.Y);
			}
		}
	}

	public string PickedY {
		get {
			return FormatCoord(PickedPoint.Y);
		}
		set {
			var newVal = Convert.ToDouble(value);
			if(PickedPoint.Y != newVal) {
				PickedPoint = new Point(PickedPoint.X, newVal);
			}
		}
	}

	Point _pickedPoint;
	public Point PickedPoint {
		get {
			return _pickedPoint;
		}
		set {
			if(_pickedPoint != value) {
				this.RaisePropertyChanging(nameof(PickedX));
				this.RaisePropertyChanging(nameof(PickedY));
				this.RaisePropertyChanging(nameof(PickedPoint));
				_pickedPoint = value;
				this.RaisePropertyChanged(nameof(PickedX));
				this.RaisePropertyChanged(nameof(PickedY));
				this.RaisePropertyChanged(nameof(PickedPoint));
			}
		}
	}

	public static bool IsSupportedPointType(Type it)
	{
		return it.Is<Point>()
			|| it.Is<PointD>()
			|| it.Is<System.Drawing.Point>()
			|| it.Is<System.Drawing.PointF>()
		;
	}

	IDisposable SubPreviewPointerPos;
	IDisposable SubIsPickingFromPreview;
	bool IsSubscribed;

	public void Dispose()
	{
		PickingUnsubscribe();
	}
}
