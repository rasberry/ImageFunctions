# ImageFunctions Plugin #
TODO fill in usage details


# Template for new Functions
When making a new function:
* Create a folder with the name of the function
* Create two files - Function.cs and Options.cs
* Copy the templates into each file
* Change MyFunction to the name of your function

## Functions/MyFunction/Function.cs

```csharp
using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.MyFunction;

public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			Core = core,
			Layers = layers
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Register);
	}

	public bool Run(string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!Options.ParseArgs(args, Register)) {
			return false;
		}

		//TODO do the work here

		return true;
	}

	readonly Options Options = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}
```

## Functions/MyFunction/Options.cs

```csharp
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.MyFunction;

public sealed class Options : IOptions
{
	public string SomeOption;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1,"Does something interesting");
		sb.ND(1,"-myopt (number)","describe myopt here");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		//use ParseNumberPercent for parsing numbers like 0.5 or 50%
		//var parser = new ParseParams.Parser<double>((string n) => {
		//	return ExtraParsers.ParseNumberPercent(n);
		//});

		if (p.Scan<string>("-myopt", "default")
			.WhenGoodOrMissing(r => { SomeOption = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		//TODO parse any other options and maybe do checks

		return true;
	}
}
```
