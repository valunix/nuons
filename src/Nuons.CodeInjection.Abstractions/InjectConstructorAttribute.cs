using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.CodeInjection.Abstractions;

/// <summary>
/// Instructs the Nuons source generator to synthesize a constructor for the decorated class.
/// The generated constructor accepts a parameter for every field annotated with <see cref="InjectedAttribute"/> or <see cref="InjectedOptionsAttribute"/> and assigns each one.
/// </summary>
[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InjectConstructorAttribute : Attribute;
