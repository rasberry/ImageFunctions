namespace ImageFunctions.Core.ColorSpace;

public interface IColor3
{
	double C1 { get; }
	double C2 { get; }
	double C3 { get; }
	double A { get; }
}

public interface IColor4 : IColor3
{
	double C4 { get; }
}

public interface ILuma
{
	double Luma { get; }
}

public interface ILumaColorSpace
{
	ILuma GetLuma(in ColorRGBA o);
}

public interface IColor3Space
{
	IColor3 ToSpace(in ColorRGBA o);
	ColorRGBA ToNative(in IColor3 o);
}

public interface IColor4Space
{
	IColor4 ToSpace(in ColorRGBA o);
	ColorRGBA ToNative(in IColor4 o);
}

public interface IColor3Space<T> : IColor3Space where T : IColor3
{
	new T ToSpace(in ColorRGBA o);
	ColorRGBA ToNative(in T o);
}
public interface IColor4Space<T> : IColor4Space where T : IColor4
{
	new T ToSpace(in ColorRGBA o);
	ColorRGBA ToNative(in T o);
}

//TODO look at https://easyrgb.com/en/convert.php
// and http://www.brucelindbloom.com/
// and http://members.chello.at/~easyfilter/colorspace.c
// https://imagej.net/plugins/color-space-converter.html
