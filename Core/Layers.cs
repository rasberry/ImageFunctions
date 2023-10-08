using System.Collections;

namespace ImageFunctions.Core;

/// <summary>
/// Represents a stack of image layers
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
	void PushAt(int index, ICanvas layer, string name = null);

	/// <summary>
	/// Removes the layer at the given index
	/// </summary>
	/// <param name="index">The index of the layer to remove</param>
	void PopAt(int index);

	/// <summary>
	/// Finds a layer with the given name. Use startIndex to find layers with the same name
	/// </summary>
	/// <param name="name">The name of the layer to search for</param>
	/// <param name="startIndex">The index to start searching</param>
	/// <returns>A positive index number if found or -1 if not found</returns>
	int IndexOf(string name, int startIndex = 0);

	/// <summary>
	/// The number of layers in the stack
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Pushes a new layer on top of the stack
	/// </summary>
	/// <param name="layer">The Icanvas object to add</param>
	/// <param name="name">The name of the layer to add</param>
	void Push(ICanvas layer, string name = null);

	/// <summary>
	/// Moves a layer from the one index to another
	/// </summary>
	/// <param name="fromIndex">The index of the image to be moved</param>
	/// <param name="toIndex">The destination index</param>
	void Move(int fromIndex, int toIndex);
}

public class CoreLayers : ILayers, IDisposable
{
	//construction should be managed by the core project
	internal CoreLayers() {}

	public ICanvas this[int index] {
		get {
			int ix = StackIxToListIx(index);
			EnsureInRange(index,nameof(ix));
			return List[ix].Canvas;
		}
		set {
			int ix = StackIxToListIx(index);
			Evict(ix);
			List[ix] = new CanvasWithName(value, GetDefaultName());
		}
	}

	public void PushAt(int index, ICanvas layer, string name = null)
	{
		int ix = StackIxToListIx(index - 1);
		EnsureInRange(ix, nameof(index), true);
		var cwn = new CanvasWithName(layer, name ?? GetDefaultName());
		List.Insert(ix,cwn);
	}

	public void PopAt(int index)
	{
		int ix = StackIxToListIx(index);
		EnsureInRange(ix, nameof(index));
		Evict(ix);
		List.RemoveAt(ix);
	}

	public int IndexOf(string name, int startIndex = 0)
	{
		int six = StackIxToListIx(startIndex);
		EnsureInRange(six, nameof(startIndex));

		//enumerations are backwards (stack ordering)
		for(int c = List.Count - six - 1; c >= 0; c--) {
			if (List[c].Name == name) {
				return c;
			}
		}
		return -1;
	}

	public void Push(ICanvas layer, string name = null)
	{
		var cwn = new CanvasWithName(layer, name ?? GetDefaultName());
		List.Add(cwn);
	}

	public void Move(int fromIndex, int toIndex)
	{
		if (fromIndex == toIndex) { return; }

		int fix = StackIxToListIx(fromIndex);
		int tix = StackIxToListIx(toIndex);
		EnsureInRange(fix, nameof(fromIndex));
		EnsureInRange(tix, nameof(toIndex));

		var item = List[fix];
		List.RemoveAt(fix); //we're not evicting just moving
		List.Insert(tix,item);
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
			Evict(i); //Evict handles IDisposable for each layer
			List.RemoveAt(i);
		}
	}

	public IEnumerator<ICanvas> GetEnumerator()
	{
		//enumerations are backwards (stack ordering)
		int count = List.Count;
		for(int i = count - 1; i >= 0; i--) {
			yield return List[i].Canvas;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return List.GetEnumerator();
	}

	void Evict(int index)
	{
		var layer = List[index];
		if (layer.Canvas is IDisposable dis) {
			dis.Dispose();
		}
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

	string GetDefaultName()
	{
		return $"Layer-({List.Count + 1})";
	}

	int StackIxToListIx(int index)
	{
		return List.Count - index - 1;
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
