namespace Nuons.Http.Generators;

internal class RouteBuilder
{
	private const string Separator = "/";
	private const string AbsolutePrefix = "~/";

	private readonly string prefix;

	public RouteBuilder(string prefix)
	{
		this.prefix = TrimPrefix(prefix);
	}

	private static string TrimPrefix(string prefix)
		=> prefix.Trim(Separator).TrimStart(AbsolutePrefix).ToString();

	public string Build(string route)
	{
		if (route.StartsWith(AbsolutePrefix) || route.StartsWith(Separator))
		{
			return route;
		}
		else
		{
			return prefix + Separator + route;
		}
	}
}
