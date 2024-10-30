namespace ImageFunctions.Core;

public interface ICoreLog
{
	void Debug(string m);
	void Message(string m);
	void Info(string m);
	void Warning(string m);
	void Error(string m, Exception e = null);
	bool BeVerbose { get; set; }
}
