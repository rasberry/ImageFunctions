using ImageFunctions.Core;
using Rasberry.Cli;
using O = ImageFunctions.Plugin.Deform.Options;

namespace ImageFunctions.Plugin.Deform;

[InternalRegisterFunction(nameof(Deform))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		if (layers == null) {
			throw Core.Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}

		if (!Tools.Engine.TryNewCanvasFromLayers(layers, out var newCanvas)) {
			return false;
		}
		var frame = layers.First();
		using var progress = new ProgressBar();
		using var canvas = newCanvas; //temporary canvas

		double ccx,ccy;
		if (O.CenterPx != null) {
			ccx = O.CenterPx.Value.X;
			ccy = O.CenterPx.Value.Y;
		}
		else {
			ccx = frame.Width * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.X);
			ccy = frame.Height * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.Y);
		}

		Tools.ThreadPixels(frame, (x,y) => {
			var nc = ProjectPixel(frame,x,y,ccx,ccy,O.Power);
			canvas[x,y] = nc;
		},progress);

		frame.CopyFrom(canvas);
		return true;
	}

	ColorRGBA ProjectPixel(ICanvas frame,double x, double y,double ccx, double ccy,double exp)
	{
		double qw = x <= ccx ? ccx : frame.Width - ccx;
		double qh = y <= ccy ? ccy : frame.Height - ccy;

		x -= ccx; y -= ccy;
		double px = 0.0, py = 0.0;

		switch(O.WhichMode)
		{
		case O.Mode.Polynomial: {
			//solve(w^q/n = w,n) : n = w^(q-1)
			double dx = Math.Pow(Math.Abs(qw),exp - 1.0);
			double dy = Math.Pow(Math.Abs(qh),exp - 1.0);
			px = Math.Sign(x) * Math.Pow(Math.Abs(x),exp) / dx;
			py = Math.Sign(y) * Math.Pow(Math.Abs(y),exp) / dy;
		}; break;
		case O.Mode.Inverted: {
			double ax = Math.Pow(Math.Abs(x),exp);
			double ay = Math.Pow(Math.Abs(y),exp);
			double aw = Math.Pow(Math.Abs(qw),exp);
			double ah = Math.Pow(Math.Abs(qh),exp);
			double num = ax + ay;
			// solve(((w^e+h^e)/w)*n=w,n);
			double dx = (qw * qw)/(aw+ah);
			double dy = (qh * qh)/(ah+aw);
			px = num / x * dx;
			py = num / y * dy;
		}; break;
		}

		px += ccx; py += ccy;

		var c = O.Sampler.Value.GetSample(frame,(int)px,(int)py);
		return c;
	}
}
/*
	public class Processor : AbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var Iis = Registry.GetImageEngine();
			var frame = Source;
			var rect = Bounds;

			using (var progress = new ProgressBar())
			using (var canvas = Iis.NewImage(rect.Width,rect.Height))
			{
				double ccx,ccy;
				if (O.CenterPx != null) {
					ccx = O.CenterPx.Value.X;
					ccy = O.CenterPx.Value.Y;
				}
				else {
					ccx = frame.Width * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.X);
					ccy = frame.Height * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.Y);
				}

				MoreHelpers.ThreadPixels(rect, MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					IColor nc = ProjectPixel(frame,x,y,ccx,ccy,O.Power);
					canvas[cx,cy] = nc;
				},progress);

				frame.BlitImage(canvas,rect);
			}
		}

		IColor ProjectPixel(IImage frame,double x, double y,double ccx, double ccy,double exp)
		{
			double qw = x <= ccx ? ccx : frame.Width - ccx;
			double qh = y <= ccy ? ccy : frame.Height - ccy;

			x -= ccx; y -= ccy;
			double px = 0.0, py = 0.0;

			switch(O.WhichMode)
			{
			case Mode.Polynomial: {
				//solve(w^q/n = w,n) : n = w^(q-1)
				double dx = Math.Pow(Math.Abs(qw),exp - 1.0);
				double dy = Math.Pow(Math.Abs(qh),exp - 1.0);
				px = Math.Sign(x) * Math.Pow(Math.Abs(x),exp) / dx;
				py = Math.Sign(y) * Math.Pow(Math.Abs(y),exp) / dy;
			}; break;
			case Mode.Inverted: {
				double ax = Math.Pow(Math.Abs(x),exp);
				double ay = Math.Pow(Math.Abs(y),exp);
				double aw = Math.Pow(Math.Abs(qw),exp);
				double ah = Math.Pow(Math.Abs(qh),exp);
				double num = ax + ay;
				// solve(((w^e+h^e)/w)*n=w,n);
				double dx = (qw * qw)/(aw+ah);
				double dy = (qh * qh)/(ah+aw);
				px = num / x * dx;
				py = num / y * dy;
			}; break;
			}

			px += ccx; py += ccy;

			var c = O.Sampler.GetSample(frame,(int)px,(int)py);
			return c;
		}

		public override void Dispose() {}
	}

*/