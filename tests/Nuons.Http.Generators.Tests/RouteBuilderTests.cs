namespace Nuons.Http.Generators.Tests;

public class RouteBuilderTests
{
	[Theory]
	[InlineData("", "", "/")]
	[InlineData("/", "", "/")]
	[InlineData("", "/", "/")]
	[InlineData("/", "/", "/")]

	[InlineData("/prefix/", "", "/prefix")]
	[InlineData("/prefix", "", "/prefix")]
	[InlineData("prefix/", "", "/prefix")]

	[InlineData("", "/route/", "/route")]
	[InlineData("", "/route", "/route")]
	[InlineData("", "route/", "/route")]

	[InlineData("/prefix/", "/route/", "/prefix/route")]
	[InlineData("prefix", "route", "/prefix/route")]
	public void RouteBuilder_BuildsRouteCorrectly(string prefix, string route, string expected)
	{
		// Arrange
		var builder = new RouteBuilder(prefix);

		// Act
		var actualRoute = builder.Build(route);

		// Assert
		actualRoute.ShouldBe(expected);
	}
}
