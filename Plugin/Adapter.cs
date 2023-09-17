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
	}
}
