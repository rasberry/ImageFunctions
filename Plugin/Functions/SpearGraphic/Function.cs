using ImageFunctions.Core;
using O = ImageFunctions.Plugin.Functions.SpearGraphic.Options;

namespace ImageFunctions.Plugin.Functions.SpearGraphic;

[InternalRegisterFunction(nameof(SpearGraphic))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}

		var img = layers.NewCanvasFromLayersOrDefault(O.DefaultWidth,O.DefaultHeight);
		layers.Push(img);

		switch(O.Spear)
		{
		case Graphic.First_Twist1:
			First.Twist1(img,img.Width,img.Height); break;
		case Graphic.First_Twist2:
			First.Twist2(img,img.Width,img.Height); break;
		case Graphic.First_Twist3:
			First.Twist3(img,img.Width,img.Height); break;
		case Graphic.Second_Twist3a:
			Second.Twist3(img,img.Width,img.Height,0); break;
		case Graphic.Second_Twist3b:
			Second.Twist3(img,img.Width,img.Height,1); break;
		case Graphic.Second_Twist3c:
			Second.Twist3(img,img.Width,img.Height,2); break;
		case Graphic.Second_Twist4:
			Second.Twist4(img,img.Width,img.Height); break;
		case Graphic.Third:
			Third.Twist1(img,img.Width,img.Width,O.RandomSeed); break;
		case Graphic.Fourth:
			Fourth.Draw(img,img.Width,img.Height,O.RandomSeed); break;
		}

		return true;
	}
}