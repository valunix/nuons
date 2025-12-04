using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.CodeInjection.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InjectConstructorAttribute : Attribute;
