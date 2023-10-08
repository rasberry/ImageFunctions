using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.GraphNet;

[InternalRegisterFunction(nameof(GraphNet))]
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
		var canvas = engine.NewCanvasFromLayersOrDefault(layers, Options.DefaultWidth, Options.DefaultHeight);
		layers.Push(canvas);

		if (O.NodeCount < 1 || O.NodeCount > canvas.Width) {
			Tell.MustBeBetween("-n","1",canvas.ToString());
			return false;
		}

		Node[] state = new Node[O.NodeCount];
		InitState(state);
		// PrintState(state);

		int maxy = canvas.Height;
		int maxx = canvas.Width;
		int maxn = O.NodeCount;
		int nodew = maxx / maxn;
		double nstates = (double)O.States;
		double prate = O.PertubationRate;

		using var progress = new ProgressBar();
		for(int y=0; y<maxy; y++) {
			int x = 0;
			for(int n=0; n<maxn; n++) {
				double g = state[n].Value / (nstates-1);
				var color = new ColorRGBA(g,g,g,1.0);
				for(int nx = 0; nx < nodew; nx++) {
					canvas[x,y] = color;
					x++;
				}
			}
			PermuteState(state,prate);
			progress.Report(y / (double)maxy);
		}

		return true;
	}

	uint[] vtemp = null;
	void PermuteState(Node[] state, double prate)
	{
		for(int n=0; n<O.NodeCount; n++) {
			uint sum = 0;
			for(int c=0; c<O.Connectivity; c++) {
				int i = state[n].Connection[c];  //get the index of the connection

				bool vkink = Rnd.NextDouble() < prate; //do we perturb value ?
				uint val = vkink ? (uint)Rnd.Next(O.States) : state[i].Value;

				bool okink = Rnd.NextDouble() < prate; //do we perturb operation ?
				PickOp op = okink ? (PickOp)Rnd.Next(PickOpCount) : state[n].Op[c];
				sum = MixValues(sum,val,op);
			}
			vtemp[n] = sum % (uint)O.States; //don't want to modify state while getting the new values
		}

		for(int n=0; n<O.NodeCount; n++) {
			state[n].Value = vtemp[n];
		}
	}

	void InitState(Node[] state)
	{
		Rnd = O.RandomSeed.HasValue
			? new Random(O.RandomSeed.Value)
			: new Random()
		;
		vtemp = new uint[O.NodeCount];

		for(int n=0; n<O.NodeCount; n++) {
			var node = new Node {
				Value = (uint)Rnd.Next(O.States), //random initial state
				Connection = new int[O.Connectivity],
				Op = new PickOp[O.Connectivity]
			};

			for(int c=0; c<O.Connectivity; c++) {
				//random connection to another node
				node.Connection[c] = Rnd.Next(O.NodeCount);
				node.Op[c] = (PickOp)Rnd.Next(PickOpCount);
			}
			state[n] = node;
		}
	}

	void PrintState(Node[] state)
	{
		var sb = new StringBuilder();
		for(int n=0; n<state.Length; n++) {
			var sn = state[n];
			Log.Message($"{n} Value={sn.Value}");
			Log.Message(PrintArray(sn.Connection));
			Log.Message(PrintArray(sn.Op));
		}
	}

	string PrintArray<T>(T[] arr)
	{
		var sb = new StringBuilder();
		sb.Append('[');
		sb.Append(arr[0]);
		for(int i=1; i<arr.Length; i++) {
			sb.Append(',');
			sb.Append(arr[i]);
		}
		sb.Append(']');
		return sb.ToString();
	}

	// https://en.wikipedia.org/wiki/Truth_table
	enum PickOp {
		/*0*/ False = 00, //contradiction
		/*1*/ Nor   = 01, //logical nor
		/*2*/ Cn    = 02, //converse nonimplication
		/*3*/ Notl  = 03, //negation left
		/*4*/ Mn    = 04, //material nonimplication
		/*5*/ Notr  = 05, //negation right
		/*6*/ Xor   = 06, //exclusive disjunction
		/*7*/ Nand  = 07, //logical nand
		/*8*/ And   = 08, //logical conjenction
		/*9*/ Xnor  = 09, //logical biconditional
		/*A*/ Pr    = 10, //projection right
		/*B*/ Mi    = 11, //material implication
		/*C*/ Pl    = 12, //projection left
		/*D*/ Ci    = 13, //converse implication
		/*E*/ Or    = 14, //logical disjunction
		/*F*/ True  = 15, //tautology
	}
	const int PickOpCount = 16;

	uint MixValues(uint left, uint rite, PickOp op)
	{
		switch(op) {
			/*0*/ case PickOp.False: return 0;
			/*1*/ case PickOp.Nor:   return ~(left | rite);
			/*2*/ case PickOp.Cn:    return ~left & ~rite;
			/*3*/ case PickOp.Notl:  return ~left;
			/*4*/ case PickOp.Mn:    return left & ~rite;
			/*5*/ case PickOp.Notr:  return ~rite;
			/*6*/ case PickOp.Xor:   return left ^ rite;
			/*7*/ case PickOp.Nand:  return ~(left & rite);

			/*8*/ case PickOp.And:   return left & rite;
			/*9*/ case PickOp.Xnor:  return ~(left ^ rite);
			/*A*/ case PickOp.Pr:    return rite;
			/*B*/ case PickOp.Mi:    return ~left | rite;
			/*C*/ case PickOp.Pl:    return left;
			/*D*/ case PickOp.Ci:    return left | ~rite;
			/*E*/ case PickOp.Or:    return left | rite;
			/*F*/ case PickOp.True:  return uint.MaxValue;
		}
		return 0;
	}

	struct Node
	{
		public uint Value;
		public int[] Connection;
		public PickOp[] Op;
	}

	Random Rnd = null;
	Options O = new Options();
}