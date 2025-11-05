using System.Collections;

namespace ImageFunctions.Plugin.Functions.Aides;

/// <summary>
/// Enumerator wrapper which allows peeking ahead.
/// </summary>
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
	/// Peek ahead at an upcoming element
	/// </summary>
	/// <param name="offset">Items to skip. Offset zero means the next item</param>
	/// <returns>The item value</returns>
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

	/// <inheritdoc/>
	public T Current { get; private set; }
	object IEnumerator.Current => Current;

	/// <inheritdoc/>
	public void Dispose()
	{
		FuncDispose?.Invoke();
	}

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void Reset()
	{
		Current = default;
		Source?.Reset();
	}

	readonly IEnumerator Source;
	readonly Queue<T> Buffer;
	readonly Action FuncDispose;
}
