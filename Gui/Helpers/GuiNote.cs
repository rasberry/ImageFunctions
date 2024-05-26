namespace ImageFunctions.Gui.Helpers;

public static class GuiNote
{
	public static string RegisteredItemWasNotFound(string name)
	{
		return $"Registered {name} should have been found but wasn't !!";
	}
	public static string WarningMustBeSelected(string name)
	{
		string aan = StartsWithVowel(name) ? "An" : "A";
		return $"⚠️{aan} {name} must be selected";
	}

	static bool StartsWithVowel(string text)
	{
		if(String.IsNullOrWhiteSpace(text)) {
			return false;
		}
		char one = text[0];
		bool isVowel =
			one == 'a' || one == 'A' ||
			one == 'e' || one == 'E' ||
			one == 'i' || one == 'I' ||
			one == 'o' || one == 'O' ||
			one == 'u' || one == 'U'
		;
		return isVowel;
	}
}
