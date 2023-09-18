using System.Collections;

namespace ImageFunctions.Core;

public interface ILayers : IEnumerable<ICanvas>
{
	ICanvas this[int index] { get; set; }
	void InsertAt(int index, ICanvas layer, string name = null);
	ICanvas RemoveAt(int index);
	int IndexOf(string name, int startIndex = 0);
	int Count { get; }
	ICanvas AddNew(string name = null);
}

public class Layers : ILayers
{
	public ICanvas this[int index] {
		get {
			return List[index].Canvas;
		}
		set {
			List[index] = new CanvasWithName(value, GetDefaultName());
		}
	}

	public void InsertAt(int index, ICanvas c, string name = null)
	{
		var cwn = new CanvasWithName(c, name ?? GetDefaultName());
		List.Insert(index,cwn);
	}

	public ICanvas RemoveAt(int index)
	{
		var c = List[index];
		List.RemoveAt(index);
		return c.Canvas;
	}

	public int IndexOf(string name, int startIndex = 0)
	{
		if (startIndex >= List.Count) {
			throw new ArgumentOutOfRangeException("startIndex");
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

	public int Count {
		get {
			return List.Count;
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
