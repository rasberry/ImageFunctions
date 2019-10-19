using System;

namespace test
{
	public enum FileSet
	{
		None = 0,
		OneOne, //one input, one output
		TwoOne, //two input, one output
	}

	public interface IAmTest
	{
		string[] GetImageNames();
		int CaseCount { get; }
		FileSet Set { get; }
		string[] GetArgs(int index);
	}
}