using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class AllowAnonymousAttribute : Attribute
{
	public AllowAnonymousAttribute() { }
}
