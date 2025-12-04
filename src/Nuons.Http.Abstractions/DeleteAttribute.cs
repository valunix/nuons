namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class DeleteAttribute : Attribute
{
	public DeleteAttribute(string route = "") { }
}
