using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Generators;

// TODO: Conceptually belongs in Nuons.Core.Generators
// Placed here because Nuons.Core.Generators does not yet ship a generator and is meant to be a shared project for all generators
[Generator]
internal class AssemblyMarkerGenerator : IIncrementalGenerator
{
	private const string HintName = "AssemblyHasNuonsMarker";

	private const string Source = $@"{Sources.GeneratedFileHeader}
[assembly: global::Nuons.Core.Abstractions.AssemblyHasNuons]
";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static context =>
			context.AddSource(Sources.GeneratedNameHint(HintName), SourceText.From(Source, Encoding.UTF8)));
	}
}
