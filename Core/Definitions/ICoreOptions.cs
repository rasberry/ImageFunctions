namespace ImageFunctions.Core;

public interface ICoreOptions : IOptions
{
	int? MaxDegreeOfParallelism { get; }
	IRegisteredItem<Lazy<IImageEngine>> Engine { get; }
	int? DefaultWidth { get; }
	int? DefaultHeight { get; }
}