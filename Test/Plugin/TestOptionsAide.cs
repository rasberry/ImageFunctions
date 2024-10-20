using ImageFunctions.Plugin.Aides;
using static Rasberry.Cli.ParseParams;

namespace ImageFunctions.Test;

[TestClass]
public class TestOptionsAide
{
	[TestMethod]
	[DataRow(0, false, Result.UnParsable)]
	[DataRow(0, true, Result.Good)]
	[DataRow(1, false, Result.Good)]
	[DataRow(1, true, Result.Good)]
	public void BeGreaterThanZero(int value, bool includeZero, Result result)
	{
		var ti = new Rasberry.Cli.ParseResult<int> {
			Name = "test",
			Result = Result.Good,
			Value = value
		};
		var tir = OptionsAide.BeGreaterThanZero(ti, includeZero);
		Assert.AreEqual(result, tir.Result);

		var tl = new Rasberry.Cli.ParseResult<long> {
			Name = "test",
			Result = Result.Good,
			Value = value
		};
		var tlr = OptionsAide.BeGreaterThanZero(tl, includeZero);
		Assert.AreEqual(result, tlr.Result);

		var td = new Rasberry.Cli.ParseResult<double> {
			Name = "test",
			Result = Result.Good,
			Value = value
		};
		var tdr = OptionsAide.BeGreaterThanZero(td, includeZero);
		Assert.AreEqual(result, tdr.Result);
	}

	[TestMethod]
	[DataRow(10, 1, 20, false, false, Result.Good)]
	[DataRow(0, 1, 20, false, false, Result.UnParsable)]
	[DataRow(1, 1, 20, false, false, Result.UnParsable)]
	[DataRow(1, 1, 20, true, false, Result.Good)]
	[DataRow(20, 1, 20, true, false, Result.UnParsable)]
	[DataRow(20, 1, 20, true, true, Result.Good)]
	[DataRow(21, 1, 20, true, true, Result.UnParsable)]
	public void BeBetween(int value, int low, int high, bool lI, bool hI, Result result)
	{
		var ti = new Rasberry.Cli.ParseResult<int> {
			Name = "test",
			Result = Result.Good,
			Value = value
		};
		var tir = OptionsAide.BeBetween(ti, low, high, lI, hI);
		Assert.AreEqual(result, tir.Result);
	}
}
