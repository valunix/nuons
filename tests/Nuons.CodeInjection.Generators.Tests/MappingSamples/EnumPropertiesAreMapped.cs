using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

// Reuses SampleOrder which has SampleOrderStatus enum property
[MapFrom(typeof(SampleOrder))]
public partial record SampleOrderEnumDto;
