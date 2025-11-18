namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class GetAttribute : Attribute
{
	public GetAttribute(string route = "") { }
}
