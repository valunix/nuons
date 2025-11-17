using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.DependencyInjection;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InjectedOptionsAttribute : Attribute;
