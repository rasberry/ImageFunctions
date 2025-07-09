using Rasberry.Cli;

namespace ImageFunctions.Core;

/// <summary>
/// Provides instances of common objects for use during function lifetime
/// </summary>
public interface IFunctionContext
{
	/// <summary>
	/// global item registration object
	/// </summary>
	IRegister Register { get; }

	/// <summary>
	/// image layers given to the function (may be empty)
	/// </summary>
	ILayers Layers { get; }

	/// <summary>
	/// user selected global program options
	/// </summary>
	ICoreOptions Options { get; }

	/// <summary>
	/// ICoreLog instance to use for logging
	/// </summary>
	ICoreLog Log { get; }

	/// <summary>
	/// Cancellation token to allow the user to stop the function
	/// </summary>
	CancellationToken Token { get; }

	/// <summary>
	/// IProgress to keep track of how much progress has been made
	/// The reported amount should be in range [0.0, 1.0]
	/// </summary>
	IProgressWithLabel<double> Progress { get; }
}
