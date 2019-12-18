using System;
using System.Runtime.CompilerServices;

namespace test
{
	public enum FileSet
	{
		None = 0,
		NoneOne, //no input, one output
		OneOne,  //one input, one output
		TwoOne,  //two input, one output
	}

	public interface IAmTest
	{
		ITuple[] GetImageNames();
		int CaseCount { get; }
		FileSet Set { get; }
		string[] GetArgs(int index);
	}
}