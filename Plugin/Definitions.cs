using ImageFunctions.Core;

namespace ImageFunctions.Plugin;

//Decorate IFunctions with this attribute to register
[AttributeUsage(AttributeTargets.Class)]
internal sealed class InternalRegisterFunctionAttribute : Attribute
{
	public InternalRegisterFunctionAttribute(string Name) {
		this.Name = Name;
	}

	public string Name { get; }
}
