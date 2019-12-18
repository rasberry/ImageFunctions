using System;
using System.IO;

namespace test
{
	public interface ITempFile : IDisposable
	{
		string TempFileName { get; }
	}

	internal class TempPngFile : ITempFile
	{
		public string TempFileName { get {
			if (FileName == null) {
				GenerateTempFile();
			}
			return FileName;
		}}

		string FileName = null;

		void GenerateTempFile()
		{
			string pr = Helpers.ProjectRoot;
			string tempFile = Path.GetTempFileName();
			File.Move(tempFile,tempFile+".png");
			tempFile += ".png";
			FileName = tempFile;
		}

		public void Dispose()
		{
			if (File.Exists(FileName)) {
				File.Delete(FileName);
			}
		}
	}
}