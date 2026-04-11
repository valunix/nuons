namespace Nuons.Http.Abstractions;

/// <summary>
/// Marks a method as an HTTP <b>PUT</b> endpoint. The Nuons source generator maps the method
/// to a PUT route, combining the class-level <see cref="RouteAttribute"/> prefix (if any) with
/// the route specified here.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PutAttribute : Attribute
{
	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="route">
	/// The route template for this endpoint (e.g. <c>"{id}"</c>). Defaults to an empty string.
	/// </param>
	public PutAttribute(string route = "") { }
}
