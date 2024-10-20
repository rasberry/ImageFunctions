namespace ImageFunctions.Core.Aides;

public static class TypeAide
{
	static readonly HashSet<Type> NumericTypes = new()
	{
		typeof(int), typeof(double), typeof(decimal),
		typeof(long), typeof(short), typeof(sbyte),
		typeof(byte), typeof(ulong), typeof(ushort),
		typeof(uint), typeof(float)
	};

	/// <summary>
	/// Gets the underlying type for a nullable
	/// </summary>
	/// <param name="t">the Type to unwrap</param>
	/// <returns>the underlying Type</returns>
	public static Type UnWrapNullable(this Type t)
	{
		var nullType = Nullable.GetUnderlyingType(t);
		if(nullType != null) {
			return nullType;
		}
		return t;
	}

	/// <summary>
	/// Determines if type is numerical. Only includes built-in types
	/// </summary>
	public static bool IsNumeric(this Type t)
	{
		if(t == null) { throw Squeal.ArgumentNull(nameof(t)); }
		return NumericTypes.Contains(Nullable.GetUnderlyingType(t) ?? t);
	}

	/// <summary>
	/// Determines if type is 'T'.
	/// </summary>
	public static bool Is<T>(this Type t)
	{
		if(t == null) { throw Squeal.ArgumentNull(nameof(t)); }
		return t.Equals(typeof(T));
	}
}
