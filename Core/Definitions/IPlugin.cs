namespace ImageFunctions.Core;

/// <summary>
/// Interface for plugins to extend. Use the IRegister instance to register
/// your plugin.
/// </summary>
public interface IPlugin : IDisposable
{
	void Init(IRegister register);
}