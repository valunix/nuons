namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PatchAttribute : Attribute
{
	public PatchAttribute(string route = "") { }
}
