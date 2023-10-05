using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageFunctions.Turmites
{
	public enum PickEdgeRule
	{
		None = 0,
		Wrap = 1,
		Reflect = 2
	}

	public enum PickOp
	{
		None = 0,
		L,R,U,F,N,S,E,W
	}

	public class Rule
	{
		public PickOp Operation = PickOp.None;
		public int Count = 0;
	}

	public class Options
	{
		public PickEdgeRule EdgeRule = PickEdgeRule.None;
		public IReadOnlyList<Rule> Sequence = null;
		public Point? Start = null;
		public ulong Iterations = 0;
	}
}