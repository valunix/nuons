using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

[MapFrom(typeof(SampleReadOnlyEntity))]
public partial record SampleReadOnlyEntityDto;
