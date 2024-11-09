using ImageFunctions.Core.FileIO;
using System.Text;

namespace ImageFunctions.Test;

[TestClass]
public class TestFileClerk
{
	string ReadToString(Stream readStream)
	{
		var rdata = new MemoryStream();
		readStream.CopyTo(rdata);
		string text = Encoding.UTF8.GetString(rdata.ToArray());
		return text;
	}

	[TestMethod]
	public void BasicFeatures()
	{
		string fileName = @"c:\temp\test.png";
		var io = new TestIO();
		using var clerk = new FileClerk(io, fileName);

		Assert.AreEqual(fileName, clerk.GetLabel(fileName));
		Assert.AreEqual(@"c:\temp\test.tiff", clerk.GetLabel(fileName, "tiff"));
		Assert.AreEqual(@"c:\temp\test-1.tiff", clerk.GetLabel(fileName, "tiff", "1"));

		var rs0 = clerk.ReadStream();
		Assert.IsTrue(rs0.CanRead);
		Assert.AreEqual("Test data 0", ReadToString(rs0));
		Assert.AreEqual(fileName, io.LastFile);
	}

	[TestMethod]
	[ExpectedException(typeof(System.InvalidOperationException))]
	public void OneStreamOnlyRead()
	{
		string fileName = @"c:\temp\test.png";
		var io = new TestIO();
		using var clerk = new FileClerk(io, fileName);

		var rs0 = clerk.ReadStream();
		Assert.AreEqual("Test data 0", ReadToString(rs0));

		//should throw InvalidOperationException "stream already created"
		var rs1 = clerk.ReadStream();
	}

	[TestMethod]
	[ExpectedException(typeof(System.InvalidOperationException))]
	public void OneStreamOnlyWrite()
	{
		string fileName = @"c:\temp\test.png";
		var io = new TestIO();
		using var clerk = new FileClerk(io, fileName);

		var rs0 = clerk.ReadStream();
		Assert.AreEqual("Test data 0", ReadToString(rs0));

		//should throw InvalidOperationException "stream already created"
		var ws1 = clerk.WriteStream();
	}

	[TestMethod]
	public void StreamFactoryWrite()
	{
		string fileName = @"c:\temp\test.png";
		var io = new TestIO();
		using var clerk = new FileClerk(io, fileName);

		var fac = clerk.WriteFactory();
		var ws0 = fac();
		Assert.AreEqual(@"c:\temp\test-1.png", io.LastFile);
		var ws1 = fac();
		Assert.AreEqual(@"c:\temp\test-2.png", io.LastFile);

		Assert.IsTrue(ws0.CanWrite);
		Assert.IsTrue(ws1.CanWrite);
		Assert.AreNotSame(ws0, ws1);
	}

	[TestMethod]
	[ExpectedException(typeof(System.InvalidOperationException))]
	public void StreamFactoryWriteOneTime()
	{
		string fileName = @"c:\temp\test.png";
		var io = new TestIO();
		using var clerk = new FileClerk(io, fileName);

		var fac0 = clerk.WriteFactory("tiff");
		var ws0 = fac0();
		Assert.AreEqual(@"c:\temp\test-1.tiff", io.LastFile);

		//should throw InvalidOperationException "factory already created"
		var fac1 = clerk.WriteFactory();
	}
}

sealed class TestIO : IFileIO
{
	int Counter = 0;
	string GetData()
	{
		return $"Test data {Counter++}";
	}

	public Stream OpenForReading(string filename)
	{
		LastFile = filename;
		var bytes = Encoding.UTF8.GetBytes(GetData());
		return new MemoryStream(bytes);
	}

	public Stream OpenForWriting(string filename)
	{
		LastFile = filename;
		return new MemoryStream();
	}

	public string LastFile;
}
