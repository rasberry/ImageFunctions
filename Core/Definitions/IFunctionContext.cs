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
}
