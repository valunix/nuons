using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class DisableAntiforgeryAttribute : Attribute
{
	public DisableAntiforgeryAttribute() { }
}
