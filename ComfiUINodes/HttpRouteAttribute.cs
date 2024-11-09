namespace ImageFunctions.ComfiUINodes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HttpRouteAttribute : Attribute
{
	public HttpRouteAttribute(string route = null)
	{
		Route = route;
	}

	public string Route;
}
