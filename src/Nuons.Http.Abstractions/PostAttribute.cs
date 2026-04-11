namespace Nuons.Http.Abstractions;

/// <summary>
/// Marks a method as an HTTP <b>POST</b> endpoint. The Nuons source generator maps the method
/// to a POST route, combining the class-level <see cref="RouteAttribute"/> prefix (if any) with
/// the route specified here.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PostAttribute : Attribute
{
	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="route">
	/// The route template for this endpoint. Defaults to an empty string.
	/// </param>
	public PostAttribute(string route = "") { }
}
