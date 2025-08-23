using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Generators.Tests;

public abstract class GeneratorTests
{
    protected string GenerateSources(GeneratorFixture fixture)
    {
        // Arrange
        var inputFile = Samples.Samples.Load();

        // Act
        var driver = fixture.Drive(inputFile);

        // Assert
        var sources = driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .Select(source => source.SourceText.ToString())
            .ToList();

        if (sources.Count != 0)
        {
            return sources.Aggregate((source1, source2) => $"{source1}{Sources.NewLine}{source2}");
        }
        else
        {
            return string.Empty;   
        }
    }

    protected void RunGenerator(ITestOutputHelper output, GeneratorFixture fixture)
    {
        var inputFile = Samples.Samples.Load();
        var driver = fixture.Drive(inputFile);
        var runResult = driver.GetRunResult();

        LogResults(output, runResult);
    }

    protected void LogResults(ITestOutputHelper output, GeneratorDriverRunResult runResult)
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