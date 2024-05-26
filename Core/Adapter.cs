using ImageFunctions.Core.Attributes;
using System.Reflection;

namespace ImageFunctions.Core;

class Adapter : IPlugin
{
	public void Dispose()
	{
	}

	public void Init(IRegister register)
	{
		//look for InternalRegister attribute and run the register method
		var assembly = this.GetType().Assembly;
		var flags = BindingFlags.Static | BindingFlags.NonPublic;
		var methods = assembly.GetTypes()
			.SelectMany(t => t.GetMethods(flags))
			.Where(m => m.GetCustomAttributes(typeof(InternalRegisterAttribute), false).Length > 0)
		;
		foreach(var m in methods) {
			//Log.Debug($"register method: {m.Name} {m.DeclaringType.Name}");
			m.Invoke(null, new object[] { register });
		}
	}
}
