using System;
using ImageFunctions.Helpers;

namespace ImageFunctions.Turmites
{
	public class Processor : AbstractProcessor
	{
		public Options O = null;

		enum Direction : int {
			N = 0,
			E = 1,
			S = 2,
			W = 3,
			Len = 4
		}

		public override void Apply()
		{
			int x,y;
			if (O.Start.HasValue) {
				x = O.Start.Value.X;
				y = O.Start.Value.Y;
			}
			else {
				x = Source.Width / 2;
				y = Source.Height / 2;
			}

			using (var progress = new ProgressBar()) {
				Draw(progress,x,y);
			}
		}

		void Draw(ProgressBar prog,int x,int y)
		{
			int width = Source.Width;
			int height = Source.Height;
			var state = new uint[Source.Width,Source.Height];
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
					Source[ix,iy] = yn ? ColorHelpers.White : ColorHelpers.Black;
				}
			}
		}

		public override void Dispose() {}
	}
}
