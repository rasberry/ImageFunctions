using ImageFunctions.Core;
using System.Reflection;

namespace ImageFunctions.Plugin;

class Adapter : IPlugin
{
	public void Dispose()
	{
	}

	public void Init(IRegister register)
	{
		//register engines
		var er = new EngineRegister(register);
		er.Add("ImageMagick", new Lazy<IImageEngine>(() => new Engines.ImageMagickEngine()));
		er.Add("SkiaSharp", new Lazy<IImageEngine>(() => new Engines.SkiaSharpEngine()));

		//register functions
		var reg = new FunctionRegister(register);
		var assembly = this.GetType().Assembly;
		var functions = SelectFunctions(assembly);
		foreach(var (name, spawn) in functions) {
			//Log.Debug($"register method: {m.Name} {m.DeclaringType.Name}");
			reg.Add(name, spawn);
		}
	}

	static IEnumerable<(string, FunctionSpawner)> SelectFunctions(Assembly assembly)
	{
		var all = assembly.GetTypes();
		foreach(var type in all) {
			var att = type.GetCustomAttributes(typeof(InternalRegisterFunctionAttribute), false);
			if(att.Length < 1) {
				continue;
			}

			var flags = BindingFlags.Static | BindingFlags.Public;
			var spawn = type.GetMethod(nameof(IFunction.Create), flags);
			var wrap = spawn.CreateDelegate<FunctionSpawner>();
			var name = ((InternalRegisterFunctionAttribute)att.First()).Name;
			yield return (name, wrap);
		}
	}
}
