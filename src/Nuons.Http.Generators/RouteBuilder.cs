using System.Text;

namespace Nuons.Http.Generators;

internal class RouteBuilder
{
	private const char Separator = '/';

	private readonly string prefix;

	public RouteBuilder(string prefix)
	{
		this.prefix = prefix.Trim(Separator);
	}

	public string Build(string route)
	{
		var builder = new StringBuilder();
		builder.Append(Separator);
		builder.Append(prefix);
		var trimmedRoute = route.Trim(Separator);
		if(prefix is not "" && trimmedRoute is not "")
		{
			builder.Append(Separator);
		}
		builder.Append(trimmedRoute);
		return builder.ToString();
	}
}
