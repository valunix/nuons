namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PutAttribute : Attribute
{
	public PutAttribute(string route = "") { }
}
