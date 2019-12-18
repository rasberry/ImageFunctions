using System;
using System.Text;
using System.Threading;

// https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54
namespace ImageFunctions
{
	/// <summary>
	/// An ASCII progress bar
	/// </summary>
	public class ProgressBar : IDisposable, IProgress<double> {

		public void Dispose() {
			lock (timer) {
				disposed = true;
				UpdateText(string.Empty);
			}
		}

		public ProgressBar() {
			timer = new Timer(TimerHandler);

			// A progress bar is only for temporary display in a console window.
			// If the console output is redirected to a file, draw nothing.
			// Otherwise, we'll end up with a lot of garbage in the target file.
			if (!Console.IsOutputRedirected) {
				ResetTimer();
			}
		}

		public void Report(double value) {
			Interlocked.Exchange(ref currentProgress, value);
		}

		public string Prefix { get; set; } = null;

		void TimerHandler(object state) {
			lock (timer) {
				if (disposed) return;

				// Make sure value is in [0..1] range
				double value = Math.Max(0, Math.Min(1, currentProgress));

				int progressBlockCount = (int) (value * blockCount);
				int percent = (int) (value * 100);
				string text = string.Format("{4}[{0}{1}] {2,3}% {3}",
					new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
					percent,
					animation[animationIndex++ % animation.Length],
					Prefix ?? ""
				);
				UpdateText(text);

				ResetTimer();
			}
		}

		void UpdateText(string text) {
			// Get length of common portion
			int commonPrefixLength = 0;
			int commonLength = Math.Min(currentText.Length, text.Length);
			while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength]) {
				commonPrefixLength++;
			}

			// Backtrack to the first differing character
			StringBuilder outputBuilder = new StringBuilder();
			outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

			// Output new suffix
			outputBuilder.Append(text.Substring(commonPrefixLength));

			// If the new text is shorter than the old one: delete overlapping characters
			int overlapCount = currentText.Length - text.Length;
			if (overlapCount > 0) {
				outputBuilder.Append(' ', overlapCount);
				outputBuilder.Append('\b', overlapCount);
			}

			Console.Write(outputBuilder);
			currentText = text;
		}

		void ResetTimer() {
			timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
		}

		const int blockCount = 10;
		const string animation = @"|/-\";
		readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
		readonly Timer timer;
		double currentProgress = 0;
		string currentText = string.Empty;
		bool disposed = false;
		int animationIndex = 0;
	}
}