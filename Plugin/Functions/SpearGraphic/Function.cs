using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.SpearGraphic;

public delegate void DrawLineFunc(ICanvas canvas, ColorRGBA color, PointD start, PointD end, double width = 1.0);

[InternalRegisterFunction(nameof(SpearGraphic))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, ICoreOptions core, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}

		var engine = core.Engine.Item.Value;
		var (dfw,dfh) = core.GetDefaultWidthHeight(Options.DefaultWidth,Options.DefaultHeight);
		var img = engine.NewCanvasFromLayersOrDefault(layers, dfw, dfh);
		layers.Push(img);

		//tell functions how to draw a line instead of them having to figure it out
		var dlf = new DrawLineFunc(engine.DrawLine);

		switch(O.Spear)
		{
		case Graphic.First_Twist1:
			First.Twist1(img,img.Width,img.Height); break;
		case Graphic.First_Twist2:
			First.Twist2(img,img.Width,img.Height); break;
		case Graphic.First_Twist3:
			First.Twist3(img,img.Width,img.Height); break;
		case Graphic.Second_Twist3a:
			Second.Twist3(img,dlf,img.Width,img.Height,0); break;
		case Graphic.Second_Twist3b:
			Second.Twist3(img,dlf,img.Width,img.Height,1); break;
		case Graphic.Second_Twist3c:
			Second.Twist3(img,dlf,img.Width,img.Height,2); break;
		case Graphic.Second_Twist4:
			Second.Twist4(img,dlf,img.Width,img.Height); break;
		case Graphic.Third:
			Third.Twist1(img,dlf,img.Width,img.Width,O.RandomSeed); break;
		case Graphic.Fourth:
			Fourth.Draw(img,dlf,img.Width,img.Height,O.RandomSeed); break;
		}

		return true;
	}

	Options O = new Options();
}