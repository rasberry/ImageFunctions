using RazorEngineCore;


namespace ImageFunctions.Writer;

public class RazorWriter
{
	public void Test()
	{
		RazorEngine engine = new RazorEngine();
		var text = File.ReadAllText(Path.Combine("Views","test.md.razor"));
		var template = engine.Compile(text);
		string txt = template.Run(new MyModel());
		Console.WriteLine(txt);

		var text2 = File.ReadAllText(Path.Combine("Views","test.chtml"));
		var template2 = engine.Compile(text2);
		string txt2 = template2.Run();
		Console.WriteLine(txt2);

	}
}

// the model class. this is 100% specific to your context
public class MyModel
{
	// this will map to @Model.Name
	public string Name => "Killroy";
}
/*
public class WriterTemplate : RazorEngineTemplateBase
{
	public MarkdownTable
}
*/

/*
// using RazorEngine.Templating; // Dont forget to include this.
string template = "Hello @Model.Name, welcome to RazorEngine!";
string templateFile = "C:/mytemplate.cshtml"
var result =
	Engine.Razor.RunCompile(new LoadedTemplateSource(template, templateFile), "templateKey", null, new { Name = "World" });

var config = new TemplateServiceConfiguration();
// .. configure your instance

var service = RazorEngineService.Create(config);

*/