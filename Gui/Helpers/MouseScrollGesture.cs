#nullable enable
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Diagnostics;

namespace ImageFunctions.Gui.Helpers;

// TODO seems like mouse should be supported out of the box
// https://github.com/AvaloniaUI/Avalonia/issues/13645

public class MouseScrollGesture : GestureRecognizer
{
	static MouseScrollGesture()
	{
		var tapHeight = Application.Current?.PlatformSettings?.GetTapSize(PointerType.Touch).Height ?? 20;
		s_defaultScrollStartDistance = (int)(tapHeight / 2);
	}

	// Pixels per second speed that is considered to be the stop of inertial scroll
	internal const double InertialScrollSpeedEnd = 5;
	public const double InertialResistance = 0.15;

	private bool _canHorizontallyScroll;
	private bool _canVerticallyScroll;
	private bool _isScrollInertiaEnabled;
	private readonly static int s_defaultScrollStartDistance;
	private int _scrollStartDistance = s_defaultScrollStartDistance;

	private bool _scrolling;
	private Point _trackedRootPoint;
	private IPointer? _tracking;
	private int _gestureId;
	private Point _pointerPressedPoint;
	private MyVelocityTracker? _velocityTracker;
	private Visual? _rootTarget;

	// Movement per second
	private Vector _inertia;
	private ulong? _lastMoveTimestamp;

	/// <summary>
	/// Defines the <see cref="CanHorizontallyScroll"/> property.
	/// </summary>
	public static readonly DirectProperty<ScrollGestureRecognizer, bool> CanHorizontallyScrollProperty =
		AvaloniaProperty.RegisterDirect<ScrollGestureRecognizer, bool>(nameof(CanHorizontallyScroll),
			o => o.CanHorizontallyScroll, (o, v) => o.CanHorizontallyScroll = v);

	/// <summary>
	/// Defines the <see cref="CanVerticallyScroll"/> property.
	/// </summary>
	public static readonly DirectProperty<ScrollGestureRecognizer, bool> CanVerticallyScrollProperty =
		AvaloniaProperty.RegisterDirect<ScrollGestureRecognizer, bool>(nameof(CanVerticallyScroll),
			o => o.CanVerticallyScroll, (o, v) => o.CanVerticallyScroll = v);

	/// <summary>
	/// Defines the <see cref="IsScrollInertiaEnabled"/> property.
	/// </summary>
	public static readonly DirectProperty<ScrollGestureRecognizer, bool> IsScrollInertiaEnabledProperty =
		AvaloniaProperty.RegisterDirect<ScrollGestureRecognizer, bool>(nameof(IsScrollInertiaEnabled),
			o => o.IsScrollInertiaEnabled, (o, v) => o.IsScrollInertiaEnabled = v);

	/// <summary>
	/// Defines the <see cref="ScrollStartDistance"/> property.
	/// </summary>
	public static readonly DirectProperty<ScrollGestureRecognizer, int> ScrollStartDistanceProperty =
		AvaloniaProperty.RegisterDirect<ScrollGestureRecognizer, int>(nameof(ScrollStartDistance),
			o => o.ScrollStartDistance, (o, v) => o.ScrollStartDistance = v,
			unsetValue: s_defaultScrollStartDistance);

	/// <summary>
	/// Gets or sets a value indicating whether the content can be scrolled horizontally.
	/// </summary>
	public bool CanHorizontallyScroll {
		get => _canHorizontallyScroll;
		set => SetAndRaise(CanHorizontallyScrollProperty, ref _canHorizontallyScroll, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the content can be scrolled vertically.
	/// </summary>
	public bool CanVerticallyScroll {
		get => _canVerticallyScroll;
		set => SetAndRaise(CanVerticallyScrollProperty, ref _canVerticallyScroll, value);
	}

	/// <summary>
	/// Gets or sets whether the gesture should include inertia in it's behavior.
	/// </summary>
	public bool IsScrollInertiaEnabled {
		get => _isScrollInertiaEnabled;
		set => SetAndRaise(IsScrollInertiaEnabledProperty, ref _isScrollInertiaEnabled, value);
	}

	/// <summary>
	/// Gets or sets a value indicating the distance the pointer moves before scrolling is started
	/// </summary>
	public int ScrollStartDistance {
		get => _scrollStartDistance;
		set => SetAndRaise(ScrollStartDistanceProperty, ref _scrollStartDistance, value);
	}

	protected override void PointerPressed(PointerPressedEventArgs e)
	{
		// Trace.WriteLine($"PointerPressed {e.ClickCount}");
		EndGesture();
		_tracking = e.Pointer;
		_gestureId = ScrollGestureEventArgs.GetNextFreeId();
		_rootTarget = (Visual?)(Target as Visual)?.GetVisualRoot();
		_trackedRootPoint = _pointerPressedPoint = e.GetPosition(_rootTarget);
		_velocityTracker = new MyVelocityTracker();
		_velocityTracker?.AddPosition(TimeSpan.FromMilliseconds(e.Timestamp), default);
	}

	protected override void PointerMoved(PointerEventArgs e)
	{
		//Trace.WriteLine($"PointerMoved {e.Pointer.Type}");
		if(e.Pointer == _tracking) {
			var rootPoint = e.GetPosition(_rootTarget);
			//Trace.WriteLine($"PointerMoved {rootPoint}");
			if(!_scrolling) {
				if(CanHorizontallyScroll && Math.Abs(_trackedRootPoint.X - rootPoint.X) > ScrollStartDistance) {
					_scrolling = true;
				}
				if(CanVerticallyScroll && Math.Abs(_trackedRootPoint.Y - rootPoint.Y) > ScrollStartDistance) {
					_scrolling = true;
				}
				if(_scrolling) {
					// Correct _trackedRootPoint with ScrollStartDistance, so scrolling does not start with a skip of ScrollStartDistance
					_trackedRootPoint = new Point(
						_trackedRootPoint.X - (_trackedRootPoint.X >= rootPoint.X ? ScrollStartDistance : -ScrollStartDistance),
						_trackedRootPoint.Y - (_trackedRootPoint.Y >= rootPoint.Y ? ScrollStartDistance : -ScrollStartDistance));

					Capture(e.Pointer);
				}
			}

			if(_scrolling) {
				var vector = _trackedRootPoint - rootPoint;

				_velocityTracker?.AddPosition(TimeSpan.FromMilliseconds(e.Timestamp), _pointerPressedPoint - rootPoint);

				_lastMoveTimestamp = e.Timestamp;
				Target!.RaiseEvent(new ScrollGestureEventArgs(_gestureId, vector));
				_trackedRootPoint = rootPoint;
				e.Handled = true;
			}
		}
	}

	protected override void PointerCaptureLost(IPointer pointer)
	{
		if(pointer == _tracking) {
			EndGesture();
		}
	}

	void EndGesture()
	{
		//Trace.WriteLine($"EndGesture");
		_tracking = null;
		if(_scrolling) {
			_inertia = default;
			_scrolling = false;
			Target!.RaiseEvent(new ScrollGestureEndedEventArgs(_gestureId));
			_gestureId = 0;
			_lastMoveTimestamp = null;
			_rootTarget = null;
		}

	}


	protected override void PointerReleased(PointerReleasedEventArgs e)
	{
		//Trace.WriteLine($"PointerReleased {e.Pointer.Type}");
		if(e.Pointer == _tracking && _scrolling) {
			_inertia = _velocityTracker?.GetFlingVelocity().PixelsPerSecond ?? Vector.Zero;

			e.Handled = true;
			if(_inertia == default
				|| e.Timestamp == 0
				|| _lastMoveTimestamp == 0
				|| e.Timestamp - _lastMoveTimestamp > 200
				|| !IsScrollInertiaEnabled)
			{
				EndGesture();
			}
			else {
				_tracking = null;
				var savedGestureId = _gestureId;
				var st = Stopwatch.StartNew();
				var lastTime = TimeSpan.Zero;
				Target!.RaiseEvent(new MyScrollGestureInertiaStartingEventArgs(_gestureId, _inertia));
				DispatcherTimer.Run(() => {
					// Another gesture has started, finish the current one
					if(_gestureId != savedGestureId) {
						return false;
					}

					var elapsedSinceLastTick = st.Elapsed - lastTime;
					lastTime = st.Elapsed;

					var speed = _inertia * Math.Pow(InertialResistance, st.Elapsed.TotalSeconds);
					var distance = speed * elapsedSinceLastTick.TotalSeconds;
					var scrollGestureEventArgs = new ScrollGestureEventArgs(_gestureId, distance);
					Target!.RaiseEvent(scrollGestureEventArgs);

					if(!scrollGestureEventArgs.Handled || scrollGestureEventArgs.ShouldEndScrollGesture) {
						EndGesture();
						return false;
					}

					// EndGesture using InertialScrollSpeedEnd only in the direction of scrolling
					if(CanVerticallyScroll && CanHorizontallyScroll && Math.Abs(speed.X) < InertialScrollSpeedEnd && Math.Abs(speed.Y) <= InertialScrollSpeedEnd) {
						EndGesture();
						return false;
					}
					else if(CanVerticallyScroll && Math.Abs(speed.Y) <= InertialScrollSpeedEnd) {
						EndGesture();
						return false;
					}
					else if(CanHorizontallyScroll && Math.Abs(speed.X) < InertialScrollSpeedEnd) {
						EndGesture();
						return false;
					}

					return true;
				}, TimeSpan.FromMilliseconds(16), DispatcherPriority.Background);
			}
		}
	}
}


readonly record struct MyVelocity(Vector PixelsPerSecond)
{
	public MyVelocity ClampMagnitude(double minValue, double maxValue)
	{
		Debug.Assert(minValue >= 0.0);
		Debug.Assert(maxValue >= 0.0 && maxValue >= minValue);
		double valueSquared = PixelsPerSecond.SquaredLength;
		if(valueSquared > maxValue * maxValue) {
			double length = PixelsPerSecond.Length;
			return new MyVelocity(length != 0.0 ? (PixelsPerSecond / length) * maxValue : Vector.Zero);
			// preventing double.NaN in Vector PixelsPerSecond is important -- if a NaN eventually gets into a
			// ScrollGestureEventArgs it results in runtime errors.
		}
		if(valueSquared < minValue * minValue) {
			double length = PixelsPerSecond.Length;
			return new MyVelocity(length != 0.0 ? (PixelsPerSecond / length) * minValue : Vector.Zero);
		}
		return this;
	}
}

/// A two dimensional velocity estimate.
///
/// VelocityEstimates are computed by [VelocityTracker.getVelocityEstimate]. An
/// estimate's [confidence] measures how well the velocity tracker's position
/// data fit a straight line, [duration] is the time that elapsed between the
/// first and last position sample used to compute the velocity, and [offset]
/// is similarly the difference between the first and last positions.
///
/// See also:
///
///  * [VelocityTracker], which computes [VelocityEstimate]s.
///  * [Velocity], which encapsulates (just) a velocity vector and provides some
///    useful velocity operations.
record MyVelocityEstimate(Vector PixelsPerSecond, double Confidence, TimeSpan Duration, Vector Offset);

record struct PointAtTime(bool Valid, Vector Point, TimeSpan Time);

/// Computes a pointer's velocity based on data from [PointerMoveEvent]s.
///
/// The input data is provided by calling [addPosition]. Adding data is cheap.
///
/// To obtain a velocity, call [getVelocity] or [getVelocityEstimate]. This will
/// compute the velocity based on the data added so far. Only call these when
/// you need to use the velocity, as they are comparatively expensive.
///
/// The quality of the velocity estimation will be better if more data points
/// have been received.
internal class MyVelocityTracker
{
	private const int AssumePointerMoveStoppedMilliseconds = 40;
	private const int HistorySize = 20;
	private const int HorizonMilliseconds = 100;
	private const int MinSampleSize = 3;
	private const double MinFlingVelocity = 50.0; // Logical pixels / second
	private const double MaxFlingVelocity = 8000.0;

	private readonly PointAtTime[] _samples = new PointAtTime[HistorySize];
	private int _index = 0;

	/// <summary>
	/// Adds a position as the given time to the tracker.
	/// </summary>
	/// <param name="time"></param>
	/// <param name="position"></param>
	public void AddPosition(TimeSpan time, Vector position)
	{
		_index++;
		if(_index == HistorySize) {
			_index = 0;
		}
		_samples[_index] = new PointAtTime(true, position, time);
	}

	/// Returns an estimate of the velocity of the object being tracked by the
	/// tracker given the current information available to the tracker.
	///
	/// Information is added using [addPosition].
	///
	/// Returns null if there is no data on which to base an estimate.
	protected virtual MyVelocityEstimate? GetVelocityEstimate()
	{
		Span<double> x = stackalloc double[HistorySize];
		Span<double> y = stackalloc double[HistorySize];
		Span<double> w = stackalloc double[HistorySize];
		Span<double> time = stackalloc double[HistorySize];
		int sampleCount = 0;
		int index = _index;

		var newestSample = _samples[index];
		if(!newestSample.Valid) {
			return null;
		}

		var previousSample = newestSample;
		var oldestSample = newestSample;

		// Starting with the most recent PointAtTime sample, iterate backwards while
		// the samples represent continuous motion.
		do {
			var sample = _samples[index];
			if(!sample.Valid) {
				break;
			}

			double age = (newestSample.Time - sample.Time).TotalMilliseconds;
			double delta = Math.Abs((sample.Time - previousSample.Time).TotalMilliseconds);
			previousSample = sample;
			if(age > HorizonMilliseconds || delta > AssumePointerMoveStoppedMilliseconds) {
				break;
			}

			oldestSample = sample;
			var position = sample.Point;
			x[sampleCount] = position.X;
			y[sampleCount] = position.Y;
			w[sampleCount] = 1.0;
			time[sampleCount] = -age;
			index = (index == 0 ? HistorySize : index) - 1;

			sampleCount++;
		} while(sampleCount < HistorySize);

		var offset = newestSample.Point - oldestSample.Point;
		var duration = newestSample.Time - oldestSample.Time;

		if(sampleCount >= MinSampleSize) {
			var xFit = LeastSquaresSolver.Solve(2, time.Slice(0, sampleCount), x.Slice(0, sampleCount), w.Slice(0, sampleCount));
			if(xFit != null) {
				var yFit = LeastSquaresSolver.Solve(2, time.Slice(0, sampleCount), y.Slice(0, sampleCount), w.Slice(0, sampleCount));
				if(yFit != null) {
					return new MyVelocityEstimate( // convert from pixels/ms to pixels/s
						PixelsPerSecond: new Vector(xFit.Coefficients[1] * 1000, yFit.Coefficients[1] * 1000),
						Confidence: xFit.Confidence * yFit.Confidence,
						Duration: duration,
						Offset: offset
					);
				}
			}
		}
		else if(sampleCount > 1) {
			// Return linear velocity if we don't have enough samples
			var distance = newestSample.Point - oldestSample.Point;
			return new MyVelocityEstimate(
				PixelsPerSecond: new Vector(distance.X / duration.Milliseconds * 1000, distance.Y / duration.Milliseconds * 1000),
				Confidence: 1,
				Duration: duration,
				Offset: offset
			);
		}

		// We're unable to make a velocity estimate but we did have at least one
		// valid pointer position.
		return new MyVelocityEstimate(
			PixelsPerSecond: Vector.Zero,
			Confidence: 1.0,
			Duration: duration,
			Offset: offset
		);
	}

	/// <summary>
	/// Computes the velocity of the pointer at the time of the last
	/// provided data point.
	///
	/// This can be expensive. Only call this when you need the velocity.
	///
	/// Returns [Velocity.zero] if there is no data from which to compute an
	/// estimate or if the estimated velocity is zero./// 
	/// </summary>
	/// <returns></returns>
	internal MyVelocity GetVelocity()
	{
		var estimate = GetVelocityEstimate();
		if(estimate == null || estimate.PixelsPerSecond == default(Vector)) {
			return new MyVelocity(Vector.Zero);
		}
		return new MyVelocity(estimate.PixelsPerSecond);
	}

	internal virtual MyVelocity GetFlingVelocity()
	{
		return GetVelocity().ClampMagnitude(MinFlingVelocity, MaxFlingVelocity);
	}
}

/// An nth degree polynomial fit to a dataset.
internal sealed class PolynomialFit
{
	/// Creates a polynomial fit of the given degree.
	///
	/// There are n + 1 coefficients in a fit of degree n.
	internal PolynomialFit(int degree)
	{
		Coefficients = new double[degree + 1];
	}

	/// The polynomial coefficients of the fit.
	public double[] Coefficients { get; }

	/// An indicator of the quality of the fit.
	///
	/// Larger values indicate greater quality.
	public double Confidence { get; set; }
}

internal sealed class LeastSquaresSolver
{
	private const double PrecisionErrorTolerance = 1e-10;

	/// <summary>
	/// Fits a polynomial of the given degree to the data points.
	/// When there is not enough data to fit a curve null is returned.
	/// </summary>
	public static PolynomialFit? Solve(int degree, ReadOnlySpan<double> x, ReadOnlySpan<double> y, ReadOnlySpan<double> w)
	{
		if(degree > x.Length) {
			// Not enough data to fit a curve.
			return null;
		}

		PolynomialFit result = new PolynomialFit(degree);

		// Shorthands for the purpose of notation equivalence to original C++ code.
		int m = x.Length;
		int n = degree + 1;

		// Expand the X vector to a matrix A, pre-multiplied by the weights.
		_Matrix a = new _Matrix(m, stackalloc double[n * m]);
		for(int h = 0; h < m; h += 1) {
			a[0, h] = w[h];
			for(int i = 1; i < n; i += 1) {
				a[i, h] = a[i - 1, h] * x[h];
			}
		}

		// Apply the Gram-Schmidt process to A to obtain its QR decomposition.

		// Orthonormal basis, column-major order Vector.
		_Matrix q = new _Matrix(m, stackalloc double[n * m]);
		// Upper triangular matrix, row-major order.
		_Matrix r = new _Matrix(n, stackalloc double[n * n]);
		for(int j = 0; j < n; j += 1) {
			for(int h = 0; h < m; h += 1) {
				q[j, h] = a[j, h];
			}
			for(int i = 0; i < j; i += 1) {
				double dot = Multiply(q.GetRow(j), q.GetRow(i));
				for(int h = 0; h < m; h += 1) {
					q[j, h] = q[j, h] - dot * q[i, h];
				}
			}

			double norm = Norm(q.GetRow(j));
			if(norm < PrecisionErrorTolerance) {
				// Vectors are linearly dependent or zero so no solution.
				return null;
			}

			double inverseNorm = 1.0 / norm;
			for(int h = 0; h < m; h += 1) {
				q[j, h] = q[j, h] * inverseNorm;
			}
			for(int i = 0; i < n; i += 1) {
				r[j, i] = i < j ? 0.0 : Multiply(q.GetRow(j), a.GetRow(i));
			}
		}

		// Solve R B = Qt W Y to find B. This is easy because R is upper triangular.
		// We just work from bottom-right to top-left calculating B's coefficients.
		// "m" isn't expected to be bigger than HistorySize=20, so allocation on stack is safe.
		Span<double> wy = stackalloc double[m];
		for(int h = 0; h < m; h += 1) {
			wy[h] = y[h] * w[h];
		}
		for(int i = n - 1; i >= 0; i -= 1) {
			result.Coefficients[i] = Multiply(q.GetRow(i), wy);
			for(int j = n - 1; j > i; j -= 1) {
				result.Coefficients[i] -= r[i, j] * result.Coefficients[j];
			}
			result.Coefficients[i] /= r[i, i];
		}

		// Calculate the coefficient of determination (confidence) as:
		//   1 - (sumSquaredError / sumSquaredTotal)
		// ...where sumSquaredError is the residual sum of squares (variance of the
		// error), and sumSquaredTotal is the total sum of squares (variance of the
		// data) where each has been weighted.
		double yMean = 0.0;
		for(int h = 0; h < m; h += 1) {
			yMean += y[h];
		}
		yMean /= m;

		double sumSquaredError = 0.0;
		double sumSquaredTotal = 0.0;
		for(int h = 0; h < m; h += 1) {
			double term = 1.0;
			double err = y[h] - result.Coefficients[0];
			for(int i = 1; i < n; i += 1) {
				term *= x[h];
				err -= term * result.Coefficients[i];
			}
			sumSquaredError += w[h] * w[h] * err * err;
			double v = y[h] - yMean;
			sumSquaredTotal += w[h] * w[h] * v * v;
		}

		result.Confidence = sumSquaredTotal <= PrecisionErrorTolerance ? 1.0 :
								1.0 - (sumSquaredError / sumSquaredTotal);

		return result;
	}

	private static double Multiply(Span<double> v1, Span<double> v2)
	{
		double result = 0.0;
		for(int i = 0; i < v1.Length; i += 1) {
			result += v1[i] * v2[i];
		}
		return result;
	}

	private static double Norm(Span<double> v)
	{
		return Math.Sqrt(Multiply(v, v));
	}

	private readonly ref struct _Matrix
	{
		private readonly int _columns;
		private readonly Span<double> _elements;

		internal _Matrix(int cols, Span<double> elements)
		{
			_columns = cols;
			_elements = elements;
		}

		public double this[int row, int col] {
			get => _elements[row * _columns + col];
			set => _elements[row * _columns + col] = value;
		}

		public Span<double> GetRow(int row) => _elements.Slice(row * _columns, _columns);
	}
}

public sealed class MyScrollGestureInertiaStartingEventArgs : RoutedEventArgs
{
	public int Id { get; }
	public Vector Inertia { get; }

	internal MyScrollGestureInertiaStartingEventArgs(int id, Vector inertia) : base(Gestures.ScrollGestureInertiaStartingEvent)
	{
		Id = id;
		Inertia = inertia;
	}
}
