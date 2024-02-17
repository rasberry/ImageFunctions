using ImageFunctions.Core;

namespace ImageFunctions.Gui.Models;

public class EngineWrapper : IImageEngine
{
	public EngineWrapper(IRegisteredItem<Lazy<IImageEngine>> engineItem)
	{
		Engine = engineItem.Item;
		AsRegisteredItem = new RegisterWrapper(this, engineItem);
	}

	public IEnumerable<ImageFormat> Formats()
	{
		return Engine.Value.Formats();
	}

	public void LoadImage(ILayers layers, string path, string name = null)
	{
		Engine.Value.LoadImage(layers, path, name);
	}

	public ICanvas NewCanvas(int width, int height)
	{
		var canvas = Engine.Value.NewCanvas(width,height);
		return new CanvasWrapper(canvas);
	}

	public void SaveImage(ILayers layers, string path, string format = null)
	{
		Engine.Value.SaveImage(layers, path, format);
	}

	class RegisterWrapper : IRegisteredItem<Lazy<IImageEngine>>
	{
		public RegisterWrapper(EngineWrapper parent, IRegisteredItem<Lazy<IImageEngine>> original)
		{
			Name = original.Name;
			NameSpace = original.NameSpace;
			Parent = parent;
		}
		readonly EngineWrapper Parent;

		public Lazy<IImageEngine> Item { get { return Parent.Engine; }}
		public string Name { get; init; }
		public string NameSpace { get; init; }
	}

	public IRegisteredItem<Lazy<IImageEngine>> AsRegisteredItem { get; init; }
	public Lazy<IImageEngine> Engine { get; init; }
}