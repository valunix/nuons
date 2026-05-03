using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.DependencyInjection.Abstractions;

/// <summary>
/// Marks a class so that the Nuons source generator produces the boilerplate needed to bind it to a named configuration section and register it with the DI container as an <c>IOptions&lt;T&gt;</c> instance.
/// </summary>
[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class OptionsAttribute : Attribute
{
	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="sectionKey">
	/// The configuration section key (e.g. <c>"MyFeature"</c>) that the options class will be bound to.
	/// Corresponds to a top-level key in <c>appsettings.json</c> or another registered configuration source.
	/// </param>
	public OptionsAttribute(string sectionKey) { }

	/// <summary>
	/// When <c>true</c>, the generator emits a registration that runs DataAnnotations validation on the options instance the first time it is resolved. Independent from <see cref="ValidateOnStart"/>.
	/// </summary>
	public bool Validate { get; init; }

	/// <summary>
	/// When <c>true</c>, the generator emits a registration that forces validation at application startup via <c>Microsoft.Extensions.Hosting</c>'s <c>ValidateOnStart()</c>. Independent from <see cref="Validate"/>; requires <c>Microsoft.Extensions.Hosting</c> at runtime.
	/// </summary>
	public bool ValidateOnStart { get; init; }
}
