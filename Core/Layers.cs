using System.Collections;

namespace ImageFunctions.Core;

public interface ILayers : IEnumerable<ICanvas>
{
	ICanvas this[int index] { get; set; }
	void InsertAt(int index, ICanvas c);
	ICanvas RemoveAt(int index);
	int IndexOf(string name);
	void Add(ICanvas c);
	int Count { get; }
}

public class Layers : ILayers
{
	public ICanvas this[int index] {
		get {
			return List[index];
		}
		set {
			List[index] = value;
		}
	}

	public void InsertAt(int index, ICanvas c)
	{
		List.Insert(index,c);
	}

	public ICanvas RemoveAt(int index)
	{
		var c = List[index];
		List.RemoveAt(index);
		return c;
	}

	public int IndexOf(string name)
	{
		for(int c = 0; c < List.Count; c++) {
			if (List[c].Name == name) {
				return c;
			}
		}
		return -1;
	}

	public void Add(ICanvas c)
	{
		List.Add(c);
	}

	public int Count {
		get {
			return List.Count;
		}
	}

	public IEnumerator<ICanvas> GetEnumerator()
	{
		return List.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return List.GetEnumerator();
	}

	List<ICanvas> List = new();
}