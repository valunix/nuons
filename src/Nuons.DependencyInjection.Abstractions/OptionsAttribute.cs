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
}
