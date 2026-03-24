using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

// Uses SampleOrder — the extension method is the focus of this test's snapshot
[MapFrom(typeof(SampleOrder))]
public partial record SampleOrderExtDto;
