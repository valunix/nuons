using System.Diagnostics;
using System.Reflection;
using Nuons.Core.Abstractions;

namespace Nuons.DependencyInjection.Abstractions.Tests;

public class AttributeConditionalTests
{
	[Fact]
	public void AllCustomAttributes_HaveConditionalAttribute_WithCorrectCondition()
	{
		// Arrange
		var assembly = typeof(DIAbstractionsAssemblyMarker).Assembly;
		var attributeTypes = assembly.GetTypes()
			.Where(type => type.IsSubclassOf(typeof(Attribute)))
			.Where(type => type.Namespace is not null && type.Namespace.StartsWith("Nuons"))
			.ToList();

		// Assert
		attributeTypes.Count.ShouldBeGreaterThan(0, "No custom attributes found in the assembly");

		// Act
		foreach (var attributeType in attributeTypes)
		{
			var conditionalAttribute = attributeType.GetCustomAttribute<ConditionalAttribute>();

			// Assert
			conditionalAttribute.ShouldNotBeNull($"{attributeType.Name} should have ConditionalAttribute");
			conditionalAttribute.ConditionString.ShouldBe(
				Constants.CodeGenerationCondition,
				$"{attributeType.Name} should have ConditionalAttribute with condition '{Constants.CodeGenerationCondition}'");
		}
	}
}
