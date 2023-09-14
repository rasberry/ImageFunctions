namespace ImageFunctions.Core;

public interface IFormatGuide
{
	IEnumerable<string> ListFormatNames();
	string GetFormatDescription(string formatName);
}