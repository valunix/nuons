using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EnableRateLimitingAttribute : Attribute
{
	public EnableRateLimitingAttribute(string policy) { }
}
