using ImageFunctions.Plugin.Functions.Aides;

namespace ImageFunctions.Test.Plugin;

[TestClass]
public class TestOther
{
	[TestMethod]
	public void TestNoPeek()
	{
		var data = Data;
		int count = 0;
		var tor = new LookAheadEnumerator<char>(data.GetEnumerator());

		while(tor.MoveNext()) {
			var item = tor.Current;
			Assert.AreEqual(data[count], item);
			count++;
		}
	}

	[TestMethod]
	public void TestWithPeek()
	{
		var data = Data;
		var tor = new LookAheadEnumerator<char>(data.GetEnumerator());

		int count = 0;
		while(tor.MoveNext()) {
			if(count == 0) {
				Assert.AreEqual('B', tor.Peek());
				Assert.AreEqual('G', tor.Peek(5));
			}

			var item = tor.Current;
			if(count < data.Length - 1) {
				Assert.AreEqual(data[count + 1], tor.Peek());
			}
			Assert.AreEqual(data[count], item);
			count++;
		}
	}

	static readonly char[] Data = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };
}
