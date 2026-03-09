using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromQueryAttribute : Attribute
{
	public FromQueryAttribute(string name = "") { }
}
