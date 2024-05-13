namespace ImageFunctions.Test;

public static class TestSqueal
{
	public static Exception FileNotFound(string path) {
		return new FileNotFoundException($"Could not find file {path}",path);
	}
}