namespace ImageFunctions.Core;

/// <summary>
/// IOptions interface used purely for consistenty within this code base
/// </summary>
public interface IOptions
{
	bool ParseArgs(string[] args, IRegister register);
	void Usage(StringBuilder sb, IRegister register);
}