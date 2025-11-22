namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PostAttribute : Attribute
{
	public PostAttribute(string route = "") { }
}
