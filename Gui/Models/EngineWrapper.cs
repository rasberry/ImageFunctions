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

	public void LoadImage(ILayers layers, IFileClerk clerk, string name = null)
	{
		//Engine.Value.LoadImage(layers, path, name);
	}

	public ICanvas NewCanvas(int width, int height)
	{
		var canvas = Engine.Value.NewCanvas(width, height);
		return new CanvasWrapper(canvas);
	}

	public void SaveImage(ILayers layers, IFileClerk clerk, string format = null)
	{
		//Engine.Value.SaveImage(layers, path, format);
	}

	public IRegisteredItem<Lazy<IImageEngine>> AsRegisteredItem { get; init; }
	public Lazy<IImageEngine> Engine { get; init; }

	class RegisterWrapper : IRegisteredItem<Lazy<IImageEngine>>
	{
		public RegisterWrapper(EngineWrapper parent, IRegisteredItem<Lazy<IImageEngine>> original)
		{
			Name = original.Name;
			NameSpace = original.NameSpace;
			Item = new Lazy<IImageEngine>(parent);
		}

		public Lazy<IImageEngine> Item { get; init; }
		public string Name { get; init; }
		public string NameSpace { get; init; }
	}
}
