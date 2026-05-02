namespace Nuons.Core.Abstractions;

/// <summary>
/// Marks an assembly as containing types decorated with Nuons attributes.
/// The Nuons source generators use this marker to locate assemblies that require code generation during the build.
/// This attribute is emitted automatically by the Nuons source generator for every project that consumes the <c>Nuons</c> package.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class AssemblyHasNuonsAttribute : Attribute;
