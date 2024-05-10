using System.Collections;
using System.Diagnostics;

namespace ImageFunctions.Core;

/// <summary>
/// Represents a stack of image layers
/// </summary>
public interface ILayers : IStackList<ISingleLayerItem>
{
	/// <summary>
	/// Insert a layer at a specific index and shifts other layers
	/// </summary>
	/// <param name="index">The index to use for the insert</param>
	/// <param name="layer">The ICanvas image object to insert</param>
	/// <param name="name">The name of the layer</param>
	/// <returns>The added object</returns>
	ISingleLayerItem PushAt(int index, ICanvas layer, string name = null);

	/// <summary>
	/// Destroys the layer at the given index. Use this to permanently delete the layer
	/// </summary>
	/// <param name="index">The index of the layer to remove</param>
	void DisposeAt(int index);

	/// <summary>
	/// Finds a layer with the given name. Use startIndex to find layers with the same name
	/// </summary>
	/// <param name="name">The name of the layer to search for</param>
	/// <param name="startIndex">The index to start searching</param>
	/// <returns>A positive index number if found or -1 if not found</returns>
	int IndexOf(string name, int startIndex = 0);

	/// <summary>
	/// Finds a layer with the unique id.
	/// </summary>
	/// <param name="id">The id of the layer to search for</param>
	/// <param name="startIndex">The index to start searching</param>
	/// <returns>A positive index number if found or -1 if not found</returns>
	int IndexOf(uint id, int startIndex = 0);

	/// <summary>
	/// Pushes a new layer on top of the stack
	/// </summary>
	/// <param name="layer">The Icanvas object to add</param>
	/// <param name="name">The name of the layer to add</param>
	/// <returns>The added object</returns>
	ISingleLayerItem Push(ICanvas layer, string name = null);
}

/// <summary>
/// Represents a single layer
/// </summary>
public interface ISingleLayerItem
{
	/// <summary>The stored canvas</summary>
	ICanvas Canvas { get; }
	/// <summary>The associated name</summary>
	string Name { get; }
	/// <summary>The unique Id for this layer</summary>
	uint Id { get; }
}

public sealed class Layers : ILayers, IDisposable
{
	//construction should be managed by the core project
	//internal Layers() {}

	public ISingleLayerItem this[int index] {
		get {
			return Stack[index];
		}
		set {
			var item = Stack[index];
			Stack[index] = new SingleLayerItem(value.Canvas, value.Name);
			if (item.Canvas is IDisposable dis) {
				dis.Dispose();
			}
		}
	}

	public ISingleLayerItem PushAt(int index, ICanvas layer, string name = null)
	{
		var cwn = new SingleLayerItem(layer, name);
		Stack.PushAt(index, cwn);
		return cwn;
	}

	public ISingleLayerItem PopAt(int index)
	{
		return Stack.PopAt(index);
	}

	public ISingleLayerItem Push(ICanvas layer, string name = null)
	{
		var cwn = new SingleLayerItem(layer, name);
		Stack.Push(cwn);
		return cwn;
	}

	public void DisposeAt(int index)
	{
		var item = Stack.PopAt(index);
		if (item.Canvas is IDisposable dis) {
			dis.Dispose();
		}
	}

	public int IndexOf(string name, int startIndex = 0)
	{
		//if there's nothing in the list no match possible
		if (Stack.Count < 1) { return -1; }

		//enumerations are backwards (stack ordering)
		for(int c = startIndex; c < Stack.Count; c++) {
			if (Stack[c].Name == name) {
				return c;
			}
		}
		return -1;
	}

	public int IndexOf(uint id, int startIndex = 0)
	{
		//if there's nothing in the list no match possible
		if (Stack.Count < 1) { return -1; }

		for(int c = startIndex; c < Stack.Count; c++) {
			if (Stack[c].Id == id) {
				return c;
			}
		}

		return -1;
	}

	public void Move(int fromIndex, int toIndex) => Stack.Move(fromIndex, toIndex);
	public int Count => Stack.Count;

	public void Dispose()
	{
		int count = Stack.Count;
		//Note: this only works because StackList is really a list and the iteration is reversed.
		for(int i = 0; i < count; i++) {
			var item = Stack.PopAt(0); //we always pop at zero since that's the same as removing the last item
			if (item.Canvas is IDisposable dis) {
				dis.Dispose();
			}
		}
		GC.SuppressFinalize(this);
	}

	public IEnumerator<ISingleLayerItem> GetEnumerator() => Stack.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	void IStackList<ISingleLayerItem>.Push(ISingleLayerItem item) => Stack.Push(item);
	void IStackList<ISingleLayerItem>.AddRange(IEnumerable<ISingleLayerItem> items) => Stack.AddRange(items);
	ISingleLayerItem IStackList<ISingleLayerItem>.Pop() => Stack.Pop();
	void IStackList<ISingleLayerItem>.PushAt(int index, ISingleLayerItem item) => Stack.PushAt(index,item);

	//readonly List<SingleLayerItem> List = new();
	readonly StackList<ISingleLayerItem> Stack = new();

	class SingleLayerItem : ISingleLayerItem
	{
		public SingleLayerItem(ICanvas canvas, string name = null)
		{
			Id = Interlocked.Increment(ref Counter);
			Canvas = canvas;
			Name = name ?? $"Layer-{Id}";
		}

		/// <summary>The stored canvas</summary>
		public ICanvas Canvas { get; init; }
		/// <summary>The associated name</summary>
		public string Name { get; init; }
		/// <summary>The unique Id for this layer</summary>
		public uint Id { get; init; }

		static uint Counter = 0;
	}
}
