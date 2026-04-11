using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.DependencyInjection.Abstractions;

/// <summary>
/// Marks a class for automatic registration as a <b>transient</b> service.
/// The Nuons source generator will register the class as both the service type and the implementation type, i.e. <c>services.AddTransient&lt;TImpl&gt;()</c>.
/// </summary>
[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TransientAttribute : Attribute;

/// <summary>
/// Marks a class for automatic registration as a <b>transient</b> service mapped to the specified service type, i.e. <c>services.AddTransient&lt;TService, TImpl&gt;()</c>.
/// </summary>
/// <typeparam name="TService">The service type (typically an interface) that the implementation will be registered under.</typeparam>
[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TransientAttribute<TService> : Attribute;
