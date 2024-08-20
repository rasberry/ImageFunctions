namespace ImageFunctions.Core.Aides;

public static class MathAide
{
	//https://en.wikipedia.org/wiki/Sinc_function
	public static double SinC(double v)
	{
		if(Math.Abs(v) < double.Epsilon) {
			return 1.0;
		}
		v *= Math.PI; //normalization factor
		double s = Math.Sin(v) / v;
		return Math.Abs(s) < double.Epsilon ? 0.0 : s;
	}

	/// <summary>
	/// Turns a number between 0 and 9 into the word
	/// </summary>
	/// <param name="number">A number between 0 and 9 (inclusive)</param>
	/// <returns>The word</returns>
	/// <exception cref="ArgumentOutOfRangeException">When the number is not supported</exception>
	public static string NumberToWord(int number)
	{
		switch(number) {
		case 0: return "zero";
		case 1: return "one";
		case 2: return "two";
		case 3: return "three";
		case 4: return "four";
		case 5: return "five";
		case 6: return "six";
		case 7: return "seven";
		case 8: return "eight";
		case 9: return "nine";
		}
		throw Squeal.ArgumentOutOfRange(nameof(number));
	}
}