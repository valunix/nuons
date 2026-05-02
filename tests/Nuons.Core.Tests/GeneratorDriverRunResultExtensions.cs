using Microsoft.CodeAnalysis;

namespace Nuons.Core.Tests;

public static class GeneratorDriverRunResultExtensions
{
	public static void Log(this GeneratorDriverRunResult runResult, ITestOutputHelper output)
	{
		output.WriteLine("------------------------------------ START errors ------------------------------------");
		runResult.Results
			.Select(result => result.Exception)
			.OfType<Exception>()
			.Select(e => e.Message)
			.ToList()
			.ForEach(message =>
			{
				output.WriteLine(message);
				output.WriteLine(string.Empty);
			});
		output.WriteLine("------------------------------------ END errors ------------------------------------");

		output.WriteLine(string.Empty);

		output.WriteLine("------------------------------------ START sources ------------------------------------");
		runResult.Results
			.SelectMany(result => result.GeneratedSources)
			.Select(source => source.SourceText)
			.ToList()
			.ForEach(sourceText =>
			{
				output.WriteLine(sourceText.ToString());
				output.WriteLine(string.Empty);
			});
		output.WriteLine("------------------------------------ END sources ------------------------------------");
	}
}
