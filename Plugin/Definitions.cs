using ImageFunctions.Core;

namespace ImageFunctions.Plugin;

/// <summary>
/// IOptions interface used purely for consistenty within this code base
/// </summary>
public interface IOptions
{
	static abstract bool ParseArgs(string[] args, IRegister register);
	static abstract void Usage(StringBuilder sb);
}

//Decorate IFunctions with this attribute to register
[AttributeUsage(AttributeTargets.Class)]
internal sealed class InternalRegisterFunctionAttribute : Attribute
{
	public InternalRegisterFunctionAttribute(string Name) {
		this.Name = Name;
	}

	public string Name { get; }
}
