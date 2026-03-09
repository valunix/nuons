using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class AuthorizeAttribute : Attribute
{
	public AuthorizeAttribute(string policy = "") { }
}
