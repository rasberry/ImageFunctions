namespace ImageFunctions.Core;

static class ExitCode
{
	public const int Success = 0;
	public const int StoppedAtParseArgs = 1;
	public const int StoppedAtProcessOptions = 2;
	public const int StoppedAtLoadImages = 3;
	public const int StoppedFunctionNotRegistered = 4;
	public const int StoppedAfterRun = 5;
}
