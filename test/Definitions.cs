using System;
using System.Drawing;
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
		int CaseCount { get; }
		FileSet Set { get; }
		string[] GetArgs(int index);

	}

	public interface IAmTestSomeOne : IAmTest
	{
		ITuple[] GetImageNames();
	}

	public interface IAmTestNoneOne : IAmTest
	{
		string GetOutName(int index);
		Rectangle? GetBounds(int index);
	}
}