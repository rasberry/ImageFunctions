using System.Collections;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

public sealed class LookAheadEnumerator<T> : IEnumerator<T>
{
	public LookAheadEnumerator(IEnumerator<T> source)
	{
		ArgumentNullException.ThrowIfNull(source);
		Source = source;
		//Plain IEnumerables doesn't have Dispose, so tracking the generic version
		FuncDispose = source.Dispose;
		Buffer = new Queue<T>();
	}

	public LookAheadEnumerator(IEnumerator source)
	{
		ArgumentNullException.ThrowIfNull(source);
		Source = source;
		Buffer = new Queue<T>();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="offset"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public T Peek(int offset = 0)
	{
		if(offset < 0) {
			throw new ArgumentOutOfRangeException(nameof(offset));
		}

		while(Buffer.Count <= offset && Source.MoveNext()) {
			Buffer.Enqueue((T)Source.Current);
		}

		if(Buffer.Count <= offset) {
			throw new IndexOutOfRangeException(nameof(offset));
		}

		return Buffer.ElementAt(offset);
	}

	public T Current { get; private set; }
	object IEnumerator.Current => Current;

	public void Dispose()
	{
		FuncDispose?.Invoke();
	}

	public bool MoveNext()
	{
		if(Buffer.Count > 0) {
			var item = Buffer.Dequeue();
			Current = item;
			return true;
		}
		else if(Source.MoveNext()) {
			Current = (T)Source.Current;
			return true;
		}
		else {
			return false;
		}
	}

	public void Reset()
	{
		Current = default;
		Source?.Reset();
	}

	readonly IEnumerator Source;
	readonly Queue<T> Buffer;
	readonly Action FuncDispose;
}