#if false
using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class ImageFormatRegister : AbstractRegistrant<Lazy<IFunction>>
{
	public ImageFormatRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal override string Namespace { get { return "ImageFormat"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		//TODO register Functions here
	}
}
#endif
