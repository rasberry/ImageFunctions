using System.Collections;

namespace ImageFunctions.Core;

/// <summary>
/// Represents a list of image layers
/// </summary>
public interface ILayers : IEnumerable<ICanvas>
{
	/// <summary>
	/// Get or Set an individual layer
	/// </summary>
	/// <param name="index">The index of the layer</param>
	/// <returns>A ICanvas image object</returns>
	ICanvas this[int index] { get; set; }

	/// <summary>
	/// Insert a layer at a specific index and shifts other layers
	/// </summary>
	/// <param name="index">The index to use for the insert</param>
	/// <param name="layer">The ICanvas image object to insert</param>
	/// <param name="name">The name of the layer</param>
	void InsertAt(int index, ICanvas layer, string name = null);

	/// <summary>
	/// Removes the layer at the given index
	/// </summary>
	/// <param name="index">The index of the layer to remove</param>
	void RemoveAt(int index);

	/// <summary>
	/// Finds a layer with the given name. Use startIndex to find layers with the same name
	/// </summary>
	/// <param name="name">The name of the layer to search for</param>
	/// <param name="startIndex">The index to start searching</param>
	/// <returns>A positive index number if found or -1 if not found</returns>
	int IndexOf(string name, int startIndex = 0);

	/// <summary>
	/// The number of layers in the list
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Appends a new layer to the end of the list
	/// </summary>
	/// <param name="layer">The Icanvas object to add</param>
	/// <param name="name">The name of the layer to add</param>
	void Add(ICanvas layer, string name = null);

	/// <summary>
	/// Moves a layer from the one index to another
	/// </summary>
	/// <param name="fromIndex">The index of the image to be moved</param>
	/// <param name="toIndex">The destination index</param>
	void Move(int fromIndex, int toIndex);
}

public class Layers : ILayers, IDisposable
{
	//construction should be managed by the core project
	internal Layers() {}

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
			throw Squeal.ArgumentOutOfRange(nameof(startIndex));
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
			Squeal.IndexOutOfRange(name);
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
