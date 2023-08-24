using ImageFunctions.Core;

namespace ImageFunctions.Plugin;

public class Adapter : IPlugin
{
	public void Dispose()
	{
	}

	public void Init(IRegister register)
	{
		var a = System.Reflection.Assembly.GetExecutingAssembly();
		register.RegisterAll(a);
	}
}
