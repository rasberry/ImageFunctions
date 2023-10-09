namespace ImageFunctions.Core;

public interface ICoreOptions : IOptions
{
	int? MaxDegreeOfParallelism { get; }
	//string OutputName { get; }
	//string ImageFormat { get; }
	//string FunctionName { get; }
	//string[] FunctionArgs { get; }
	//IReadOnlyList<string> ImageFileNames { get; }
	IRegisteredItem<Lazy<IImageEngine>> Engine { get; }
	int? DefaultWidth { get; }
	int? DefaultHeight { get; }
}