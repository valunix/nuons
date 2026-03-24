using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

[MapFrom(typeof(SampleOrder))]
[MapIgnore("InternalTrackingId")]
public partial record SampleOrderWithIgnoreDto;
