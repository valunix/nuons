namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class RouteAttribute : Attribute
{
	public RouteAttribute(string route = "") { }
}
