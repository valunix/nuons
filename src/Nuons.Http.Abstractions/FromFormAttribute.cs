using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromFormAttribute : Attribute
{
	public FromFormAttribute(string name = "") { }
}
