using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Deform;

[InternalRegisterFunction(nameof(Deform))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, ICoreOptions core, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}
		if (layers.Count < 1) {
			Tell.LayerMustHaveAtLeast();
			return false;
		}

		var engine = core.Engine.Item.Value;
		using var canvas = engine.NewCanvasFromLayers(layers); //temporary canvas
		var frame = layers.First();
		using var progress = new ProgressBar();

		double ccx,ccy;
		if (O.CenterPx != null) {
			ccx = O.CenterPx.Value.X;
			ccy = O.CenterPx.Value.Y;
		}
		else {
			ccx = frame.Width * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.X);
			ccy = frame.Height * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.Y);
		}

		int maxThreads = core.MaxDegreeOfParallelism.GetValueOrDefault(1);
		Tools.ThreadPixels(frame, (x,y) => {
			var nc = ProjectPixel(frame,x,y,ccx,ccy,O.Power);
			canvas[x,y] = nc;
		},maxThreads,progress);

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
		case Options.Mode.Polynomial: {
			//solve(w^q/n = w,n) : n = w^(q-1)
			double dx = Math.Pow(Math.Abs(qw),exp - 1.0);
			double dy = Math.Pow(Math.Abs(qh),exp - 1.0);
			px = Math.Sign(x) * Math.Pow(Math.Abs(x),exp) / dx;
			py = Math.Sign(y) * Math.Pow(Math.Abs(y),exp) / dy;
		}; break;
		case Options.Mode.Inverted: {
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

	Options O = new Options();
}
