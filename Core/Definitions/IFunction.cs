using System.Text;

namespace ImageFunctions.Core;

public interface IFunction
{
	/// <summary>
	/// Print out the usage and options of the function
	/// </summary>
	/// <param name="sb">Add lines to the given StringBuilder</param>
	void Usage(StringBuilder sb);

	/// <summary>
	/// Run the function
	/// </summary>
	/// <param name="layers">image layers given to the function (may be empty)</param>
	/// <param name="args">command like style arguments</param>
	/// <returns>true on success, false on failure which will stop further processing</returns>
	bool Run(ILayers layers, string[] args);
}