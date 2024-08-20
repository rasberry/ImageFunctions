namespace ImageFunctions.Core.Aides;

public static class IgnoreCaseAide
{
	public static bool EqualsIC(this string sub, string test)
	{
		if(sub == null) { return false; }
		return sub.Equals(test, StringComparison.OrdinalIgnoreCase);
	}

	public static bool StartsWithIC(this string sub, string test)
	{
		if(sub == null) { return false; }
		return sub.StartsWith(test, StringComparison.OrdinalIgnoreCase);
	}

	public static bool EndsWithIC(this string sub, string test)
	{
		if(sub == null) { return false; }
		return sub.EndsWith(test, StringComparison.OrdinalIgnoreCase);
	}
}
