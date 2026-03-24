using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

// Non-partial record — the generator should produce NO output
[MapFrom(typeof(SampleOrder))]
public record SampleNonPartialDto;
