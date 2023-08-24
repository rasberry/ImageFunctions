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

public interface IColorLuma
{
	double Luma { get; }
}

public interface ILumaColorSpace
{
	IColorLuma GetLuma(in IColor o);
}

public interface I3ColorSpace
{
	I3Color ToSpace(in IColor o);
	IColor ToNative(in I3Color o);
}

public interface I4ColorSpace
{
	I4Color ToSpace(in IColor o);
	IColor ToNative(in I4Color o);
}