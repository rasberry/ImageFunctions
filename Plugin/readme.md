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
	public static IFunction Create(IFunctionContext context)
	{
		if(context == null) {
			throw Squeal.ArgumentNull(nameof(context));
		}

		var f = new Function {
			Context = context,
			Local = new(context),
		};
		return f;
	}

	public void Usage(StringBuilder sb)
	{
		Core.Usage(sb, Context.Register);
	}

	public bool Run(string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!Core.ParseArgs(args, Context.Register)) {
			return false;
		}
		if(Layers.Count < 1) {
			Context.Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		//TODO do the work here

		return true;
	}

	public IOptions Core { get { return Local; } }
	public ILayers Layers { get { return Context.Layers; } }
	Options Local;
	IFunctionContext Context;
}
```

## Functions/MyFunction/Options.cs

```csharp
using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.MyFunction;

public sealed class Options : IOptions
{
	public string SomeOption;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1, "Does something interesting");
		sb.ND(1, "-myopt (number)", "describe myopt here");
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
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		//TODO parse any other options and maybe do checks

		return true;
	}
}
```
