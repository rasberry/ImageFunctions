using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Turmites;

[InternalRegisterFunction(nameof(Turmites))]
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

		var engine = core.Engine.Item.Value;
		var source = engine.NewCanvasFromLayersOrDefault(layers, Options.DefaultWidth, Options.DefaultHeight);
		layers.Push(source);

		int x,y;
		if (O.Start.HasValue) {
			x = O.Start.Value.X;
			y = O.Start.Value.Y;
		}
		else {
			x = source.Width / 2;
			y = source.Height / 2;
		}

		var progress = new ProgressBar();
		Draw(source, progress,x,y);
		return true;
	}

	void Draw(ICanvas source, ProgressBar prog,int x,int y)
	{
		int width = source.Width;
		int height = source.Height;
		var state = new uint[width,height];
		var dir = Direction.N;
		ulong seqLen = (ulong)O.Sequence.Count;

		for(ulong i=0; i<O.Iterations; i++) {
			prog.Report((double)i/O.Iterations);

			// change direction
			uint c = state[x,y];
			var rule = O.Sequence[(int)(c % seqLen)];
			switch(rule.Operation) {
				case PickOp.L: dir--; break;
				case PickOp.R: dir++; break;
				case PickOp.U: dir += 2; break;
				case PickOp.N: dir = Direction.N; break;
				case PickOp.S: dir = Direction.S; break;
				case PickOp.E: dir = Direction.E; break;
				case PickOp.W: dir = Direction.W; break;
			}

			//https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
			if (dir < Direction.N || dir > Direction.W) {
				int d = (int)dir % (int)Direction.Len;
				dir = (Direction)(d < 0 ? d + (int)Direction.Len : d);
			}

			//change state
			state[x,y]++;

			//move forward
			switch(dir) {
				case Direction.N: y--; break;
				case Direction.E: x++; break;
				case Direction.S: y++; break;
				case Direction.W: x--; break;
			}

			//handle edge condition
			if (O.EdgeRule == PickEdgeRule.None) {
				if (x > width || x < 0 || y > height || y < 0) { break; }
			}
			else if (O.EdgeRule == PickEdgeRule.Wrap) {
				if (x >= width)  { x = 0; }
				else if (x < 0)  { x = width - 1; }
				if (y >= height) { y = 0; }
				if (y < 0)       { y = height - 1; }
			}
			else if (O.EdgeRule == PickEdgeRule.Reflect) {
				if (x >= width)  { x = width - 1; dir = Direction.W; }
				else if (x < 0)  { x = 0; dir = Direction.E; }
				if (y >= height) { y = height - 1; dir = Direction.N; }
				else if (y < 0)  { y = 0; dir = Direction.S; }
			}
		}

		for(int iy=0; iy<height; iy++) {
			for(int ix=0; ix<width; ix++) {
				bool yn = state[ix,iy] % 2 == 0;
				source[ix,iy] = yn ? PlugColors.White : PlugColors.Black;
			}
		}
	}

	Options O = new Options();

	enum Direction : int {
		N = 0,
		E = 1,
		S = 2,
		W = 3,
		Len = 4
	}
}