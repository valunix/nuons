namespace Nuons.Core.Abstractions;

/// <summary>
/// Apply this attribute to an assembly to indicate that it contains types decorated with Nuons attributes.
/// The Nuons source generators use this marker to locate assemblies that require code generation during the build.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class AssemblyHasNuonsAttribute : Attribute;
