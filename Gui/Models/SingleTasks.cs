using System.Diagnostics;

namespace ImageFunctions.Gui.Models;

public static class SingleTasks
{
	public static SingleTonTask GetOrMake(string name, Action<CancellationToken> job, TimeSpan? timeout = null)
	{
		bool found = Tasks.TryGetValue(name, out var task);
		bool replace = found && (task == null || task.IsRunning == false);
		//Trace.WriteLine($"{nameof(GetOrMake)} name={name} found={found} replace={replace}");

		if(!found || replace) {
			task = new SingleTonTask {
				Job = job,
				Timeout = timeout ?? TimeSpan.FromSeconds(DefaultTimeoutSeconds)
			};

			if(replace) {
				Tasks[name] = task;
			}
			else {
				Tasks.Add(name, task);
			}
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
	readonly static Dictionary<string, SingleTonTask> Tasks = new();
}

public sealed class SingleTonTask : IDisposable
{
	public Action<CancellationToken> Job { get; set; }
	public TimeSpan Timeout { get; set; }
	public bool IsRunning { get; private set; }
	CancellationTokenSource TokenSource;
	Task RunningTask;

	/// <summary>Try to start the task</summary>
	/// <returns>true if the task was started</returns>
	public async Task<bool> Run()
	{
		//stop the currently running task if it's run again.
		if(IsRunning) {
			//Trace.WriteLine($"{nameof(SingleTonTask)} Run Calling cancel");
			TokenSource?.Cancel();
			return false;
		}

		TokenSource = new(Timeout);
		IsRunning = true;

		try {
			//Trace.WriteLine($"{nameof(SingleTonTask)} Starting Run..");
			RunningTask = Task.Run(() => Job(TokenSource.Token), TokenSource.Token);

			await RunningTask.ContinueWith(t => {
				IsRunning = false;
				TokenSource.TryReset();
			});
			//Trace.WriteLine($"{nameof(SingleTonTask)} Ending Run..");
		}
		catch(Exception e) {
			Trace.WriteLine($"Run {e.GetType().Name} thrown with message: {e.Message}");
			IsRunning = false;
			TokenSource.TryReset();
		}
		return true;
	}

	public void Cancel()
	{
		//Trace.WriteLine($"{nameof(SingleTonTask)} Cancel called IsRunning={IsRunning}");
		if(IsRunning) {
			try {
				TokenSource.Cancel();
			}
			catch(Exception e) {
				Trace.WriteLine($"Cancel {e.GetType().Name} thrown with message: {e.Message}");
			}
			finally {
				IsRunning = false;
				TokenSource.TryReset();
			}
		}
	}

	public void Dispose()
	{
		TokenSource?.Dispose();
	}
}
