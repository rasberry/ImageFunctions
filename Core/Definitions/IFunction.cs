namespace ImageFunctions.Core;

public interface IFunction
{
	/// <summary>
	/// Returns an instance of the function options
	/// </summary>
	IOptions Options { get; }

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
	/// <param name="context">context to use during function lifetime</param>
	/// <returns>IFunction instance</returns>
	abstract static IFunction Create(IFunctionContext context);
}
