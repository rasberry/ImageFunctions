using System;
using System.Text;

namespace ImageFunctions
{
	public enum Action
	{
		None = 0,
		PixelateDetails = 1,
		Derivatives = 2
	}

	public interface IFunction
	{
		void Usage(StringBuilder sb);
		bool ParseArgs(string[] args);
		void Main();
	}
}