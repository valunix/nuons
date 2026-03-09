using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromServicesAttribute : Attribute
{
	public FromServicesAttribute() { }
}
