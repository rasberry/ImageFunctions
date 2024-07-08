#if false
//TODO remove - a format register doesn't make much sense since these are tied
// closely to the engine

using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class ImageFormatRegister : AbstractRegistrant<Lazy<IFunction>>
{
	public ImageFormatRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal const string NS = "ImageFormat";
	public override string Namespace { get { return NS; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		//TODO register Functions here
	}
}
#endif
