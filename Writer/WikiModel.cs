namespace ImageFunctions.Writer;

public class WikiModel
{
	public string ProjectName { get { return "Image Functions"; } }
	public string Usage { get; set; }
	public MarkdownTable Table { get; set; }
	public string FunctionName { get; set; }
	public List<string> FunctionList { get; set; }
}
