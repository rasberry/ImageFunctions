namespace ImageFunctions.Core;

static class ExitCode
{
	public static readonly int Success = 0;
	public static readonly int StoppedAtParseArgs = 1;
	public static readonly int StoppedAtProcessOptions = 2;
	public static readonly int StoppedAtLoadImages = 3;
	public static readonly int StoppedFunctionNotRegistered = 4;
	public static readonly int StoppedAfterRun = 5;
}