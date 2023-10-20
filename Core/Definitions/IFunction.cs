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
	/// Run the function with the specified function specific arguments
	/// </summary>
	/// <param name="args">command like style arguments</param>
	/// <returns>true on success, false on failure which will stop further processing</returns>
	bool Run(string[] args);

	/// <summary>
	/// Used to get an instance of the function. The Run method will be called after Create
	/// Note: Instances should generally not persist between calls to Run
	/// </summary>
	/// <param name="register">global item registration object</param>
	/// <param name="layers">image layers given to the function (may be empty)</param>
	/// <param name="options">user selected global program options</param>
	/// <returns></returns>
	abstract static IFunction Create(IRegister register, ILayers layers, ICoreOptions options);
}