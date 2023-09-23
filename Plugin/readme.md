# ImageFunctions Plugin #
TODO fill in usage details


# Template for new Functions
When making a new function:
* Create a folder with the name of the function
* Create two files - Function.cs and Options.cs
* Copy the templates into each file
* Change MyFunction to the name of your function

## MyFunction/Functions.cs

```csharp
using ImageFunctions.Core;

namespace ImageFunctions.Plugin.MyFunction;

public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		if (layers == null) {
			throw Core.Squeal.ArgumentNull(nameof(layers));
		}
		if (!Options.ParseArgs(args, register)) {
			return false;
		}

		//TODO do the work here

		return true;
	}
}
```

## MyFunction/Options.cs

```csharp
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.MyFunction;

public sealed class Options : IOptions
{
	public static string SomeOption;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Does something interesting");
		sb.ND(1,"-myopt (number)","describe myopt here");
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Default("-myopt",out SomeOption).IsBad()) {
			return false;
		}

		//TODO parse any other options and maybe do checks

		return true;
	}
}
```