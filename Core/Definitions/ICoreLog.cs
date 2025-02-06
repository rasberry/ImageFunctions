namespace ImageFunctions.Core;

public interface ICoreLog
{
	void Debug(string m);
	void Message(string m);
	void Info(string m);
	void Warning(string m);
	void Error(string m, Exception e = null);
	LogCategory Category { get; set; }
}

//Note: these must be ordered and numbered from least severe to most severe
public enum LogCategory : int
{
	Unknown = 0,
	Debug = 1,
	Info = 2,
	Message = 3,
	Warning = 4,
	Error = 5,
	Disabled = 99
}
