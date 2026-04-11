namespace Nuons.Http.Abstractions;

/// <summary>
/// Marks a method as an HTTP <b>DELETE</b> endpoint. The Nuons source generator maps the method
/// to a DELETE route, combining the class-level <see cref="RouteAttribute"/> prefix (if any)
/// with the route specified here.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class DeleteAttribute : Attribute
{
	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="route">
	/// The route template for this endpoint (e.g. <c>"{id}"</c>). Defaults to an empty string.
	/// </param>
	public DeleteAttribute(string route = "") { }
}
