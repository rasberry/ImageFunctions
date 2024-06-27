namespace ImageFunctions.Core;

public static class TypeTool
{
	static readonly HashSet<Type> NumericTypes = new()
	{
		typeof(int), typeof(double), typeof(decimal),
		typeof(long), typeof(short), typeof(sbyte),
		typeof(byte), typeof(ulong), typeof(ushort),
		typeof(uint), typeof(float)
	};

	public static Type UnWrapNullable(this Type t)
	{
		var nullType = Nullable.GetUnderlyingType(t);
		if(nullType != null) {
			return nullType;
		}
		return t;
	}

	public static bool IsNumeric(this Type t)
	{
		return NumericTypes.Contains(Nullable.GetUnderlyingType(t) ?? t);
	}

	public static bool IsBool(this Type t)
	{
		return t.Equals(typeof(bool));
	}

	public static bool IsColorRGBA(this Type t)
	{
		return t.Equals(typeof(Core.ColorRGBA));
	}

	public static bool IsColor(this Type t)
	{
		return t.Equals(typeof(System.Drawing.Color));
	}

	public static bool IsPoint(this Type t)
	{
		return t.Equals(typeof(System.Drawing.Point));
	}

	public static bool IsString(this Type t)
	{
		return t.Equals(typeof(string));
	}


}