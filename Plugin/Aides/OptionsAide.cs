using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;
using System.Globalization;

namespace ImageFunctions.Plugin.Aides;

public static class OptionsAide
{
	/// <summary>
	/// Ensures the parameter is greater than zero.
	/// </summary>
	/// <typeparam name="T">Type parameter must be IComparable</typeparam>
	/// <param name="r">The result of the parameter parsing</param>
	/// <param name="includeZero">whether to include zero as a valid option or not</param>
	/// <returns>An updated result</returns>
	public static ParseResult<T> BeGreaterThanZero<T>(this ParseResult<T> r, ICoreLog log, bool includeZero = false)
		where T : IComparable
	{
		return BeGreaterThan<T>(r, log, default, includeZero);
	}

	/// <summary>
	/// Ensures the parameter is greater than a given number.
	/// </summary>
	/// <typeparam name="T">Type parameter must be IComparable</typeparam>
	/// <param name="r">The result of the parameter parsing</param>
	/// <param name="inclusive">whether to include the minimum as valid option or not</param>
	/// <returns>An updated result</returns>
	public static ParseResult<T> BeGreaterThan<T>(this ParseResult<T> r, ICoreLog log, T minimum, bool inclusive = false)
		where T : IComparable
	{
		if (log == null) { throw Squeal.ArgumentNull(nameof(log)); }
		if(r.IsBad()) { return r; }

		var t = typeof(T).UnWrapNullable();
		bool isInvalid = false;

		if(r.Value is double vd) {
			double min = (minimum is IConvertible c) ? c.ToDouble(CultureInfo.InvariantCulture) : 0.0;

			if((!inclusive && (vd - min) >= double.Epsilon)
				|| (inclusive && (vd - min) >= 0.0)) {
				return r with { Result = ParseParams.Result.Good };
			}
			isInvalid = true;
		}
		else if(r.Value is IComparable vi) {
			var compare = vi.CompareTo(minimum);
			if((!inclusive && compare > 0) || (inclusive && compare >= 0)) {
				return r with { Result = ParseParams.Result.Good };
			}
			isInvalid = true;
		}

		if(isInvalid) {
			log.Error(Note.MustBeGreaterThan(r.Name, minimum, inclusive));
			return r with { Result = ParseParams.Result.UnParsable };
		}
		else {
			throw PlugSqueal.NotSupportedTypeByFunc(t, nameof(BeGreaterThan));
		}
	}

	/// <summary>
	/// Ensures the parameter is between two numbers
	/// </summary>
	/// <typeparam name="T">Type parameter must be an IComparable</typeparam>
	/// <param name="r">The result of the parameter parsing</param>
	/// <param name="low">The smallest allowable value</param>
	/// <param name="high">The largest allowable value</param>
	/// <param name="lowInclusive">Whether to allow the value itself</param>
	/// <param name="highInclusive">Whether to allow the value itself</param>
	/// <returns>An updated result</returns>
	public static ParseResult<T> BeBetween<T>(this ParseResult<T> r, ICoreLog log, T low, T high,
		bool lowInclusive = true, bool highInclusive = true) where T : IComparable
	{
		if(r.IsBad()) { return r; }
		var t = typeof(T).UnWrapNullable();

		var clow = r.Value.CompareTo(low);
		var chigh = r.Value.CompareTo(high);
		if((!lowInclusive && clow > 0 || lowInclusive && clow >= 0)
			&& (!highInclusive && chigh < 0 || highInclusive && chigh <= 0)) {
			return r with { Result = ParseParams.Result.Good };
		}

		log.Error(PlugNote.MustBeInRange(r.Name, low, high, lowInclusive, highInclusive));
		return r with { Result = ParseParams.Result.UnParsable };
	}

	const int NomSize = 1024;
	/// <summary>
	/// Helpers to get the default width / height either provided by the user
	///  or provided as an input
	/// </summary>
	/// <param name="options">ICoreOptions object - usually passed to a function</param>
	/// <param name="defaultWidth">The fallback width to use</param>
	/// <param name="defaultHeight">The fallback height to use/param>
	/// <returns>A tuple with width, height</returns>
	public static (int, int) GetDefaultWidthHeight(this ICoreOptions options, int defaultWidth = NomSize, int defaultHeight = NomSize)
	{
		return (
			options.DefaultWidth.GetValueOrDefault(defaultWidth),
			options.DefaultHeight.GetValueOrDefault(defaultHeight)
		);
	}
}
