using System.Reflection;
using ImageFunctions.Core;

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
		er.Add("ImageMagick",new Lazy<IImageEngine>(() => new Engines.ImageMagickEngine()));
		er.Add("SkiaSharp",new Lazy<IImageEngine>(() => new Engines.SkiaSharpEngine()));

		//register functions
		var reg = new FunctionRegister(register);
		var assembly = this.GetType().Assembly;
		var functions = SelectFunctions(assembly);
		foreach(var f in functions) {
			//Log.Debug($"register method: {m.Name} {m.DeclaringType.Name}");
			reg.Add(f.Item1, f.Item2);
		}

		//reg.Add("AllColors",new Lazy<IFunction>(() => new AllColors.Function()));
		//reg.Add("AreaSmoother",new Lazy<IFunction>(() => new AreaSmoother.Function()));
		//reg.Add("AreaSmoother2",new Lazy<IFunction>(() => new AreaSmoother2.Function()));
	}

	static IEnumerable<(string,Lazy<IFunction>)> SelectFunctions(Assembly assembly)
	{
		var all = assembly.GetTypes();
		foreach(var type in all) {
			var att = type.GetCustomAttributes(typeof(InternalRegisterFunctionAttribute), false);
			if (att.Length < 1) {
				continue;
			}

			var wrap = new Lazy<IFunction>(() => (IFunction)Activator.CreateInstance(type));
			var name = ((InternalRegisterFunctionAttribute)att.First()).Name;
			yield return (name,wrap);
		}
	}
}
