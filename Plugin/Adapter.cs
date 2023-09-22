using ImageFunctions.Core;

namespace ImageFunctions.Plugin;

class Adapter : IPlugin
{
	public void Dispose()
	{
	}

	public void Init(IRegister register)
	{
		//var a = System.Reflection.Assembly.GetExecutingAssembly();
		var reg = new FunctionRegister(register);
		reg.Add("AllColors",new Lazy<IFunction>(() => new AllColors.Function()));
		reg.Add("AreaSmoother",new Lazy<IFunction>(() => new AreaSmoother.Function()));
		reg.Add("AreaSmoother2",new Lazy<IFunction>(() => new AreaSmoother2.Function()));

		var er = new EngineRegister(register);
		er.Add("ImageMagick",new Lazy<IImageEngine>(() => new Engines.ImageMagickEngine()));
	}
}
