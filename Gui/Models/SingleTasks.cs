using System.Diagnostics;

namespace ImageFunctions.Gui.Models;

public static class SingleTasks
{
	public static async Task TimeoutAfterAsync(this Task task, TimeSpan timeout)
	{
		using var timeoutCancel = new CancellationTokenSource();
		var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancel.Token));
		if(completedTask == task) {
			timeoutCancel.Cancel();
			await task;  // Very important in order to propagate exceptions
		}
		else {
			throw new TimeoutException("The operation has timed out.");
		}
	}

	public static SingleTonTask GetOrMake(string name, Action<CancellationToken> job, TimeSpan? timeout = null)
	{
		if(!Tasks.TryGetValue(name, out var task)) {
			task = new SingleTonTask {
				Job = job,
				Timeout = timeout ?? TimeSpan.FromSeconds(DefaultTimeoutSeconds)
			};

			Tasks.Add(name, task);
		}
		return task;
	}

	public static SingleTonTask Get(string name)
	{
		if(Tasks.TryGetValue(name, out var task)) {
			return task;
		}
		return null;
	}

	const int DefaultTimeoutSeconds = 30;
	static Dictionary<string, SingleTonTask> Tasks = new();
}

public class SingleTonTask : IDisposable
{
	public Action<CancellationToken> Job { get; set; }
	public TimeSpan Timeout { get; set; }
	public bool IsRunning { get; private set; }
	readonly CancellationTokenSource TokenSource = new();
	Task RunningTask;

	/// <summary>Try to start the task</summary>
	/// <returns>true if the task was started</returns>
	public async Task<bool> Run()
	{
		if(IsRunning) {
			TokenSource.Cancel();
			return false;
		}

		IsRunning = true;

		try {
			RunningTask = Task.Run(() => Job(TokenSource.Token), TokenSource.Token)
				.TimeoutAfterAsync(Timeout);

			await RunningTask.ContinueWith(t => {
				IsRunning = false;
				TokenSource.TryReset();
			});
		}
		catch(OperationCanceledException e) {
			Trace.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
			IsRunning = false;
			TokenSource.TryReset();
		}
		return true;
	}

	public void Cancel()
	{
		if(IsRunning) {
			TokenSource.Cancel();
		}
	}

	public void Dispose()
	{
		TokenSource?.Dispose();
		GC.SuppressFinalize(this);
	}
}
