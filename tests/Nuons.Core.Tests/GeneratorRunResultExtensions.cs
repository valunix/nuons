using Microsoft.CodeAnalysis;

namespace Nuons.Core.Tests;

public static class GeneratorRunResultExtensions
{
	public static void AssertStepReason(this GeneratorRunResult result, string stepName, IncrementalStepRunReason expectedReason)
	{
		result.TrackedSteps.ShouldContainKey(stepName);
		foreach (var (Value, Reason) in result.TrackedSteps[stepName].SelectMany(step => step.Outputs))
		{
			Reason.ShouldBe(expectedReason);
		}
	}

	public static void AssertAllStepsHaveReason(this GeneratorRunResult result, IEnumerable<string> stepNames, IncrementalStepRunReason expectedReason)
	{
		foreach (var stepName in stepNames)
		{
			result.AssertStepReason(stepName, expectedReason);
		}
	}
}
