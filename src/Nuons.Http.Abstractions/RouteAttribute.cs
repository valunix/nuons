namespace Nuons.Http.Abstractions;

/// <summary>
/// Defines the base route prefix for all HTTP endpoints in a class. The Nuons source generator
/// combines this prefix with the route specified on each individual HTTP method attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class RouteAttribute : Attribute
{
	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="route">
	/// The route prefix for the class (e.g. <c>"api/orders"</c>). Defaults to an empty string,
	/// meaning no prefix is added.
	/// </param>
	public RouteAttribute(string route = "") { }
}
