namespace ImageFunctions.Core;

/// <summary>
/// Interface for plugins to extend. Use the IRegister instance to register
/// your plugin.
/// </summary>
public interface IPlugin : IDisposable
{
	/// <summary>
	/// Init is called during the registration process so plugins can
	///  register any items.
	/// </summary>
	/// <param name="register">The IRegister instance used for registering items</param>
	void Init(IRegister register);
}
