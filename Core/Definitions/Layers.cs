using System.Collections;

namespace ImageFunctions.Core;

public interface ILayers : IEnumerable<ICanvas>
{
	ICanvas this[int index] { get; set; }
	void InsertAt(int index, ICanvas layer, string name = null);
	void RemoveAt(int index);
	int IndexOf(string name, int startIndex = 0);
	int Count { get; }
	void Add(ICanvas layer, string name = null);
	void Move(int fromIndex, int toIndex);
}

public class Layers : ILayers, IDisposable
{
	public ICanvas this[int index] {
		get {
			EnsureInRange(index,nameof(index));
			return List[index].Canvas;
		}
		set {
			List[index] = new CanvasWithName(value, GetDefaultName());
		}
	}

	public void InsertAt(int index, ICanvas layer, string name = null)
	{
		EnsureInRange(index, nameof(index), true);
		var cwn = new CanvasWithName(layer, name ?? GetDefaultName());
		List.Insert(index,cwn);
	}

	public void RemoveAt(int index)
	{
		var layer = List[index];
		List.RemoveAt(index);
		if (layer.Canvas is IDisposable dis) {
			dis.Dispose();
		}
	}

	public int IndexOf(string name, int startIndex = 0)
	{
		EnsureInRange(startIndex, nameof(startIndex));
		if (startIndex < 0 || startIndex >= List.Count) {
			throw new ArgumentOutOfRangeException(nameof(startIndex));
		}

		for(int c = startIndex; c < List.Count; c++) {
			if (List[c].Name == name) {
				return c;
			}
		}
		return -1;
	}

	public void Add(ICanvas layer, string name = null)
	{
		var cwn = new CanvasWithName(layer, name ?? GetDefaultName());
		List.Add(cwn);
	}

	public void Move(int fromIndex, int toIndex)
	{
		EnsureInRange(fromIndex, nameof(fromIndex));
		EnsureInRange(toIndex, nameof(toIndex));
		if (fromIndex == toIndex) { return; }

		var item = List[fromIndex];
		List.RemoveAt(fromIndex);
		List.Insert(toIndex,item);
	}

	void EnsureInRange(int index, string name, bool allowOnePast = false)
	{
		bool isGood = index >= 0 && (
			allowOnePast && index <= List.Count ||
			!allowOnePast && index < List.Count
		);
		if (!isGood) {
			throw new IndexOutOfRangeException(name);
		}
	}

	public int Count {
		get {
			return List.Count;
		}
	}

	public void Dispose()
	{
		//remove items from the end to start so we don't have to
		// shift everything each time.
		int count = List.Count;
		for(int i = count - 1; i >= 0; i--) {
			RemoveAt(i); //RemoveAt handles IDisposible for each layer
		}
	}

	string GetDefaultName()
	{
		return $"Layer-({List.Count + 1})";
	}

	public IEnumerator<ICanvas> GetEnumerator()
	{
		foreach(var item in List) {
			yield return item.Canvas;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return List.GetEnumerator();
	}

	List<CanvasWithName> List = new();
}

readonly struct CanvasWithName
{
	public CanvasWithName(ICanvas canvas, string name)
	{
		Canvas = canvas;
		Name = name;
	}

	public readonly ICanvas Canvas;
	public readonly string Name;
}
