using System.Diagnostics;
using ImageFunctions.Core;
using ImageFunctions.Gui.ViewModels;

namespace ImageFunctions.Gui;

public static class SingleTasks
{
	public static async Task TimeoutAfterAsync(this Task task, TimeSpan timeout)
	{
		using var timeoutCancel = new CancellationTokenSource();
		var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancel.Token));
		if (completedTask == task) {
			timeoutCancel.Cancel();
			await task;  // Very important in order to propagate exceptions
		} else {
			throw new TimeoutException("The operation has timed out.");
		}
	}

	public static SingleTonTask GetOrMake(string name, Action<CancellationToken> job, TimeSpan? timeout = null)
	{
		if (!Tasks.TryGetValue(name, out var task)) {
			task = new SingleTonTask {
				Job = job,
				Timeout = timeout ?? TimeSpan.FromSeconds(DefaultTimeoutSeconds)
			};

			Tasks.Add(name,task);
		}
		return task;
	}

	const int DefaultTimeoutSeconds = 30;
	static Dictionary<string,SingleTonTask> Tasks = new();
}

public class SingleTonTask : IDisposable
{
	public Action<CancellationToken> Job { get; set; }
	public TimeSpan Timeout { get; set; }
	public bool IsRunning { get; private set; }
	readonly CancellationTokenSource TokenSource = new();

	public async Task Run()
	{
		while (IsRunning) {
			TokenSource.Cancel();
			Thread.Sleep(100);
		}

		IsRunning = true;

		try {
			var task = Task.Run(() => Job(TokenSource.Token), TokenSource.Token)
				.TimeoutAfterAsync(Timeout);

			await task.ContinueWith(t => {
				IsRunning = false;
				TokenSource.TryReset();
			});
		}
		catch (OperationCanceledException e) {
			Trace.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
			IsRunning = false;
			TokenSource.TryReset();
		}
	}

	public void Dispose()
	{
		TokenSource?.Dispose();
		GC.SuppressFinalize(this);
	}
}