using ImageFunctions.Core;
using System.Text;

namespace ImageFunctions.Gui.Helpers;

public sealed class BasicOptions : ICoreOptions
{
	public int? MaxDegreeOfParallelism { get; set; }
	public IRegisteredItem<Lazy<IImageEngine>> Engine { get; set; }
	public int? DefaultWidth { get; set; }
	public int? DefaultHeight { get; set; }
	public IRegister Register { get; set; }

	public bool ParseArgs(string[] args, IRegister register)
	{
		throw new NotImplementedException();
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		throw new NotImplementedException();
	}
}